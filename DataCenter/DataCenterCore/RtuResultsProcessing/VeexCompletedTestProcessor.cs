﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public class VeexCompletedTestProcessor
    {
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly CommonBopProcessor _commonBopProcessor;
        private readonly SorFileRepository _sorFileRepository;
        private readonly D2RtuVeexLayer3 _d2RtuVeexLayer3;
        private readonly MsmqMessagesProcessor _msmqMessagesProcessor;
        private readonly IWcfServiceForRtu _wcfServiceForRtu;

        public VeexCompletedTestProcessor(IMyLog logFile, Model writeModel, CommonBopProcessor commonBopProcessor,
            SorFileRepository sorFileRepository, D2RtuVeexLayer3 d2RtuVeexLayer3,
            MsmqMessagesProcessor msmqMessagesProcessor, IWcfServiceForRtu wcfServiceForRtu)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _commonBopProcessor = commonBopProcessor;
            _sorFileRepository = sorFileRepository;
            _d2RtuVeexLayer3 = d2RtuVeexLayer3;
            _msmqMessagesProcessor = msmqMessagesProcessor;
            _wcfServiceForRtu = wcfServiceForRtu;
        }

        public async Task ProcessOneCompletedTest(CompletedTest completedTest, Rtu rtu, DoubleAddress rtuDoubleAddress)
        {
            var veexTest = _writeModel.VeexTests.FirstOrDefault(v => v.TestId.ToString() == completedTest.testId);
            if (veexTest == null) return;

            if (veexTest.IsOnBop)
                await CheckAndSendBopNetworkIfNeeded(completedTest, rtu, veexTest);

            // if (completedTest.result == "failed") 
            // return;

            var trace = _writeModel.Traces.First(t => t.TraceId == veexTest.TraceId);
            if (ShouldMoniResultBeSaved(completedTest, rtu, trace, veexTest.BasRefType))
                await AcceptMoniResult(rtuDoubleAddress, completedTest, veexTest, rtu, trace);
            else
            {
                _wcfServiceForRtu.NotifyUserCurrentMonitoringStep(new CurrentMonitoringStepDto()
                {
                    RtuId = rtu.Id,
                    Step = MonitoringCurrentStep.MeasurementFinished,
                    PortWithTraceDto = new PortWithTraceDto()
                    {
                        TraceId = veexTest.TraceId,
                        OtauPort = new OtauPortDto()
                        {
                            OtauId = veexTest.OtauId,
                        }
                    },
                    BaseRefType = veexTest.BasRefType,
                });
            }
        }

        private async Task CheckAndSendBopNetworkIfNeeded(CompletedTest completedTest, Rtu rtu, VeexTest veexTest)
        {
            if (completedTest.result == "failed")
            {
                if (completedTest.extendedResult == "otau_failed" ||
                    completedTest.extendedResult == "otau_not_found")
                {
                    var otauId = completedTest.failure.otauId.Substring(3);
                    var otau = _writeModel.Otaus.FirstOrDefault(o => o.Id.ToString() == otauId);
                    if (otau != null && otau.IsOk)
                    {
                        _logFile.AppendLine($@"RTU {rtu.Id.First6()} BOP {otau.Serial} state changed to broken");
                        await _commonBopProcessor.PersistBopEvent(CreateCmd(rtu.Id, otau));
                    }
                }
            }
            else
            {
                var otau = _writeModel.Otaus.FirstOrDefault(o => o.Id.ToString() == veexTest.OtauId);
                if (otau != null && !otau.IsOk)
                {
                    _logFile.AppendLine($@"RTU {rtu.Id.First6()} BOP {otau.Serial} state changed to OK");
                    await _commonBopProcessor.PersistBopEvent(CreateCmd(rtu.Id, otau));
                }
            }
        }

        private AddBopNetworkEvent CreateCmd(Guid rtuId, Otau otau)
        {
            return new AddBopNetworkEvent()
            {
                EventTimestamp = DateTime.Now,
                RtuId = rtuId,
                Serial = otau.Serial,
                OtauIp = otau.NetAddress.Ip4Address,
                TcpPort = otau.NetAddress.Port,
                IsOk = !otau.IsOk,
            };
        }

        private bool ShouldMoniResultBeSaved(CompletedTest completedTest, Rtu rtu, Trace trace, BaseRefType baseRefType)
        {
            if (completedTest.result == "failed" &&
                    (completedTest.extendedResult.StartsWith("otdr")
                    || completedTest.extendedResult.StartsWith("otau")))
                return false;

            var traceLastMeasOfThisBaseType = _writeModel.Measurements
                .LastOrDefault(m => m.TraceId == trace.TraceId && m.BaseRefType == baseRefType);
            if (traceLastMeasOfThisBaseType == null)
            {
                _logFile.AppendLine($"Should be saved as first measurement of this base type on trace {trace.Title}");
                return true; // first measurement on trace
            }

            if (IsTimeToSave(completedTest, rtu, traceLastMeasOfThisBaseType, baseRefType))
            {
                _logFile.AppendLine($"Time to save {baseRefType} measurement on trace {trace.Title}");
                return true;
            }

            var oldTraceState = trace.State;
            var newTraceState = GetNewTraceState(completedTest);

            if (oldTraceState != newTraceState)
            {
                _logFile.AppendLine($"Trace state changed: {oldTraceState} -> {newTraceState}");
                return true;
            }

            var tracePreviousMeas = _writeModel.Measurements.Last(m => m.TraceId == trace.TraceId);
            if (tracePreviousMeas.BaseRefType == BaseRefType.Fast && baseRefType != BaseRefType.Fast)
            {
                _logFile.AppendLine($"Event confirmation by {baseRefType} ref");
                return true;
            }

            return false;
        }


        private async Task AcceptMoniResult(DoubleAddress rtuDoubleAddress,
            CompletedTest completedTest, VeexTest veexTest, Rtu rtu, Trace trace)
        {
            var getSorResult = await _d2RtuVeexLayer3.GetCompletedTestSorBytesAsync(rtuDoubleAddress, completedTest.id.ToString());
            if (!getSorResult.IsSuccessful)
            {
                _logFile.AppendLine($"Failed to get sor bytes of measurements. {getSorResult.ErrorMessage}");
                return;
            }

            var res = new MonitoringResultDto()
            {
                MeasurementResult = MeasurementResult.Success,
                BaseRefType = veexTest.BasRefType == BaseRefType.Fast
                    ? BaseRefType.Fast
                    : completedTest.indicesOfReferenceTraces[0] == 0
                        ? BaseRefType.Precise
                        : BaseRefType.Additional,
                SorBytes = getSorResult.ResponseBytesArray,
            };

            var baseRef = await GetBaseRefSorBytes(veexTest.TraceId, res.BaseRefType); // from db on server
            var sorData = SorData.FromBytes(res.SorBytes);
            sorData.EmbedBaseRef(baseRef);
            res.SorBytes = sorData.ToBytes();

            res.TimeStamp = completedTest.started;
            res.RtuId = rtu.Id;
            res.PortWithTrace = new PortWithTraceDto()
            {
                TraceId = veexTest.TraceId,
                OtauPort = trace.OtauPort,
            };
            res.TraceState = GetNewTraceState(completedTest);

            await _msmqMessagesProcessor.ProcessMonitoringResult(res);
        }

        private static FiberState GetNewTraceState(CompletedTest completedTest)
        {
            if (completedTest.result == "failed")
            {
                if (completedTest.extendedResult == "trace_change")
                {
                    if (completedTest.traceChange.changeType == "exceeded_threshold")
                    {
                        if (completedTest.traceChange.levels.Any(l => l.levelName == "critical"))
                            return FiberState.Critical;
                        if (completedTest.traceChange.levels.Any(l => l.levelName == "major"))
                            return FiberState.Major;
                        return FiberState.Minor;
                    }
                    return completedTest.traceChange.changeType.ToFiberState(); // fiber_break
                }
                return completedTest.extendedResult.ToFiberState(); // no_fiber
            }
            return FiberState.Ok;
        }

        private bool IsTimeToSave(CompletedTest completedTest, Rtu rtu, Measurement traceLastMeas, BaseRefType baseRefType)
        {
            var frequency = baseRefType == BaseRefType.Fast ? rtu.FastSave : rtu.PreciseSave;
            if (frequency == Frequency.DoNot) return false;
            return completedTest.started - traceLastMeas.MeasurementTimestamp > frequency.GetTimeSpan();
        }

        private async Task<byte[]> GetBaseRefSorBytes(Guid traceId, BaseRefType baseRefType)
        {
            var baseRef = _writeModel.BaseRefs.FirstOrDefault(b => b.TraceId == traceId && b.BaseRefType == baseRefType);
            if (baseRef == null) return null;
            return await _sorFileRepository.GetSorBytesAsync(baseRef.SorFileId);
        }
    }
}