using System;
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

            foreach (var notificationEvent in veexMeasurementDto.VeexNotification.events)
            {
                await ProcessOneNotification(notificationEvent, rtu, rtuAddresses);
            }

            return true;
        }

        private async Task ProcessOneNotification(VeexNotificationEvent notificationEvent, Rtu rtu, DoubleAddress rtuAddresses)
        {
            notificationEvent.time = TimeZoneInfo.ConvertTime(notificationEvent.time, TimeZoneInfo.Local);
            var port = notificationEvent.data.OtauPorts[0].portIndex + 1;
            var testName = notificationEvent.data.testName;

            _logFile.AppendLine($"{testName} on port {port} - {notificationEvent.type} at {notificationEvent.time}", 0, 3);
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

            res.TimeStamp = notificationEvent.time;
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
            var traceLastMeasOfThisBaseType = _writeModel.Measurements
                .LastOrDefault(m => m.TraceId == trace.TraceId && m.BaseRefType == baseRefType);
            if (traceLastMeasOfThisBaseType == null)
            {
                _logFile.AppendLine($"Should be saved as first measurement of this base type on trace {trace.Title}");
                return true; // first measurement on trace
            }

            if (IsTimeToSave(notificationEvent, rtu, traceLastMeasOfThisBaseType, baseRefType))
            {
                _logFile.AppendLine($"Time to save {baseRefType} measurement on trace {trace.Title}");
                return true;
            }

            var oldTraceState = trace.State;
            var newTraceState = GetNewTraceState(notificationEvent);

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

        private FiberState GetNewTraceState(VeexNotificationEvent notificationEvent)
        {
            if (notificationEvent.type == "monitoring_test_passed") return FiberState.Ok;

            if (notificationEvent.data.extendedResult == "trace_change")
                if (notificationEvent.data.traceChange.changeType == "exceeded_threshold")
                    return notificationEvent.data.traceChange.levelName.ToFiberState();
                else
                    return notificationEvent.data.traceChange.changeType.ToFiberState();

            return notificationEvent.data.extendedResult.ToFiberState();
        }

        private bool IsTimeToSave(VeexNotificationEvent notificationEvent, Rtu rtu, Measurement traceLastMeas, BaseRefType baseRefType)
        {
            var frequency = baseRefType == BaseRefType.Fast ? rtu.FastSave : rtu.PreciseSave;
            if (frequency == Frequency.DoNot) return false;
            return notificationEvent.time - traceLastMeas.MeasurementTimestamp > frequency.GetTimeSpan();
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
