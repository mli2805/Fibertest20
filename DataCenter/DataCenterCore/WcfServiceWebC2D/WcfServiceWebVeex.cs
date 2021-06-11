﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceWebC2D
    {
        public async Task<bool> MonitoringMeasurementDone(VeexMeasurementDto veexMeasurementDto)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == veexMeasurementDto.RtuId);
            if (rtu == null) return false;
            var rtuAddresses = new DoubleAddress()
            { Main = rtu.MainChannel, HasReserveAddress = rtu.IsReserveChannelSet, Reserve = rtu.ReserveChannel };

            foreach (var notificationEvent in veexMeasurementDto.VeexNotification.Events)
            {
                await ProcessOneNotification(notificationEvent, rtu, rtuAddresses);
            }

            return true;
        }

        private async Task ProcessOneNotification(VeexNotificationEvent notificationEvent, Rtu rtu, DoubleAddress rtuAddresses)
        {
            notificationEvent.Time = TimeZoneInfo.ConvertTime(notificationEvent.Time, TimeZoneInfo.Local);
            var port = notificationEvent.Data.OtauPorts[0].PortIndex + 1;
            var testName = notificationEvent.Data.TestName;

            _logFile.AppendLine($"{testName} on port {port} - {notificationEvent.Type} at {notificationEvent.Time}");
            var trace = _writeModel.Traces.FirstOrDefault(t => t.RtuId == rtu.Id && t.Port == port);
            if (trace == null)
                return;  // no such a trace
            var baseRefType = GetBaseRefTypeFromName(testName);

            // first we need to check if monitoring result should be fetched and saved
            if (!ShouldMoniResultBeSaved(notificationEvent, rtu, trace, baseRefType)) return;

            // second: fetch moni result
            var res = await _d2RtuVeexLayer3.GetTestLastMeasurement(rtuAddresses, notificationEvent);
            if (res.MeasurementResult != MeasurementResult.Success) return;

            var baseRef = await GetBaseRefSorBytes(trace, baseRefType); // from db on server
            var sorData = SorData.FromBytes(res.SorBytes);
            sorData.EmbedBaseRef(baseRef);
            res.SorBytes = sorData.ToBytes();

            res.TimeStamp = notificationEvent.Time;
            res.RtuId = rtu.Id;
            res.BaseRefType = baseRefType;
            res.PortWithTrace = new PortWithTraceDto()
            {
                TraceId = trace.TraceId,
                OtauPort = trace.OtauPort,
            };
            res.TraceState = GetNewTraceState(notificationEvent);

            await _msmqMessagesProcessor.ProcessMonitoringResult(res);
        }

        private bool ShouldMoniResultBeSaved(VeexNotificationEvent notificationEvent, Rtu rtu, Trace trace, BaseRefType baseRefType)
        {
            var traceLastMeas = _writeModel.Measurements
                .LastOrDefault(m => m.TraceId == trace.TraceId && m.BaseRefType == baseRefType);
            if (traceLastMeas == null)
            {
                _logFile.AppendLine($"Should be saved as first measurement on trace {trace.Title}");
                return true; // first measurement on trace
            }

            if (IsTimeToSave(notificationEvent, rtu, traceLastMeas, baseRefType))
            {
                _logFile.AppendLine($"Time to save {baseRefType} measurement on trace {trace.Title}");
                return true;
            }

            var oldTraceState = trace.State;
            var newTraceState = GetNewTraceState(notificationEvent);
            _logFile.AppendLine($"Old state: {oldTraceState} - new state: {newTraceState}");
            return newTraceState != oldTraceState;
        }

        private FiberState GetNewTraceState(VeexNotificationEvent notificationEvent)
        {
            if (notificationEvent.Type == "monitoring_test_passed") return FiberState.Ok;

            if (notificationEvent.Data.ExtendedResult == "trace_change")
                if (notificationEvent.Data.TraceChange.ChangeType == "exceeded_threshold")
                    return notificationEvent.Data.TraceChange.LevelName.ToFiberState();
                else
                    return notificationEvent.Data.TraceChange.ChangeType.ToFiberState();

            return notificationEvent.Data.ExtendedResult.ToFiberState();
        }

        private bool IsTimeToSave(VeexNotificationEvent notificationEvent, Rtu rtu, Measurement traceLastMeas, BaseRefType baseRefType)
        {
            var frequency = baseRefType == BaseRefType.Fast ? rtu.FastSave : rtu.PreciseSave;
            if (frequency == Frequency.DoNot) return false;
            return notificationEvent.Time - traceLastMeas.MeasurementTimestamp > frequency.GetTimeSpan();
        }

        private BaseRefType GetBaseRefTypeFromName(string testName)
        {
            if (testName.Contains("fast")) return BaseRefType.Fast;
            if (testName.Contains("precise")) return BaseRefType.Precise;
            return BaseRefType.Additional;
        }

        private async Task<byte[]> GetBaseRefSorBytes(Trace trace, BaseRefType baseRefType)
        {
            var baseRef = _writeModel.BaseRefs.FirstOrDefault(b => b.TraceId == trace.TraceId && b.BaseRefType == baseRefType);
            if (baseRef == null) return null;
            return await _sorFileRepository.GetSorBytesAsync(baseRef.SorFileId);
        }
    }
}
