using System;
using System.Collections.Concurrent;
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
        private readonly OutOfTurnData _outOfTurnData;
        public readonly ConcurrentDictionary<Guid, string> RequestedTests = new ConcurrentDictionary<Guid, string>();

        public VeexCompletedTestProcessor(IMyLog logFile, Model writeModel, CommonBopProcessor commonBopProcessor,
            SorFileRepository sorFileRepository, D2RtuVeexLayer3 d2RtuVeexLayer3,
            MsmqMessagesProcessor msmqMessagesProcessor, IWcfServiceForRtu wcfServiceForRtu,
            OutOfTurnData outOfTurnData)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _commonBopProcessor = commonBopProcessor;
            _sorFileRepository = sorFileRepository;
            _d2RtuVeexLayer3 = d2RtuVeexLayer3;
            _msmqMessagesProcessor = msmqMessagesProcessor;
            _wcfServiceForRtu = wcfServiceForRtu;
            _outOfTurnData = outOfTurnData;
        }

        public async Task ProcessOneCompletedTest(CompletedTest completedTest, Rtu rtu, DoubleAddress rtuDoubleAddress)
        {
            var veexTest = _writeModel.VeexTests.FirstOrDefault(v => v.TestId == completedTest.testId);
            if (veexTest == null)
            {
                _logFile.AppendLine($"RTU {rtu.Title} returned unknown test {completedTest.testId.First6()}");
                return;
            }

            var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == veexTest.TraceId);
            if (trace == null) // old tests
            {
                _logFile.AppendLine($"RTU {rtu.Title} returned test {completedTest.testId.First6()} with no related trace");
                return;
            }

            _logFile.AppendLine($"RTU {rtu.Title} returned {trace.Title} monitoring result", 0, 3);

            // for both main and additional otau
            await CheckAndSendBopNetworkIfNeeded(completedTest, rtu, veexTest);

            if (ShouldMoniResultBeSaved(completedTest, rtu, trace, veexTest.BasRefType))
                await AcceptMoniResult(rtuDoubleAddress, completedTest, veexTest, rtu, trace);
            else
            {
                _wcfServiceForRtu.NotifyUserCurrentMonitoringStep(new CurrentMonitoringStepDto()
                {
                    RtuId = rtu.Id,
                    Step = completedTest.GetMonitoringCurrentStep(),
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
            try
            {
                // CompletedTest contains completedTest.failure.otauId
                // but
                var otau = veexTest.IsOnBop
                    ? _writeModel.Otaus.FirstOrDefault(o => o.Id.ToString() == veexTest.OtauId)
                    : _writeModel.Otaus.FirstOrDefault(o => o.VeexRtuMainOtauId == veexTest.OtauId);
                if (otau == null) return;
                if (completedTest.failure != null)
                    if (completedTest.failure.message == "OTDR not found" //TODO how to make sure user knows about breakdown
                        || veexTest.IsOnBop && completedTest.failure.otauId.StartsWith("S1_")  // test on Bop but failed main otau
                        || !veexTest.IsOnBop && completedTest.failure.otauId.StartsWith("S2_")) // or vise versa
                        return;

                var isOtauBroken = completedTest.result == "failed" && completedTest.extendedResult.StartsWith("otau");

                if (otau.IsOk == isOtauBroken)
                {
                    var word = isOtauBroken ? "broken" : "OK";
                    _logFile.AppendLine($@"RTU {rtu.Id.First6()} otau {otau.Serial} state changed to {word}");
                    await _commonBopProcessor.PersistBopEvent(CreateCmd(rtu.Id, otau));
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("CheckAndSendBopNetworkIfNeeded: " + e.Message);
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
            try
            {
                if (RequestedTests.ContainsKey(completedTest.testId))
                {
                    RequestedTests.TryRemove(completedTest.testId, out string _);
                    _outOfTurnData.SetRtuIsFree(rtu.Id);
                    return true;
                }

                if (completedTest.result == "failed" &&
                    (completedTest.extendedResult.StartsWith("otdr")
                     || completedTest.extendedResult.StartsWith("otau")))
                    return false;

                var traceLastMeasOfThisBaseType = _writeModel.Measurements
                    .LastOrDefault(m => m.TraceId == trace.TraceId && m.BaseRefType == baseRefType);

                // only Fast first measurement should be saved
                if (baseRefType == BaseRefType.Fast && traceLastMeasOfThisBaseType == null)
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
            }
            catch (Exception e)
            {
                _logFile.AppendLine("ShouldMoniResultBeSaved: " + e.Message);
                return false;
            }

            return false;
        }


        private async Task AcceptMoniResult(DoubleAddress rtuDoubleAddress,
            CompletedTest completedTest, VeexTest veexTest, Rtu rtu, Trace trace)
        {
            try
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
            catch (Exception e)
            {
                _logFile.AppendLine("AcceptMoniResult: " + e.Message);
            }
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
            try
            {
                var frequency = baseRefType == BaseRefType.Fast ? rtu.FastSave : rtu.PreciseSave;
                if (frequency == Frequency.DoNot) return false;
                if (traceLastMeas == null) return true;
                return completedTest.started - traceLastMeas.MeasurementTimestamp > frequency.GetTimeSpan();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("IsTimeToSave: " + e.Message);
                return false;
            }
        }

        private async Task<byte[]> GetBaseRefSorBytes(Guid traceId, BaseRefType baseRefType)
        {
            var baseRef = _writeModel.BaseRefs.FirstOrDefault(b => b.TraceId == traceId && b.BaseRefType == baseRefType);
            if (baseRef == null) return null;
            return await _sorFileRepository.GetSorBytesAsync(baseRef.SorFileId);
        }
    }
}