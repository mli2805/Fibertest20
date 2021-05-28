using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceWebC2D
    {
        public async Task<bool> MonitoringMeasurementDone(VeexMeasurementDto veexMeasurementDto)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == veexMeasurementDto.RtuId);
            if (rtu == null) return false;
            var rtuAddresses = new DoubleAddress()
                {Main = rtu.MainChannel, HasReserveAddress = rtu.IsReserveChannelSet, Reserve = rtu.ReserveChannel};

            foreach (var notificationEvent in veexMeasurementDto.VeexNotification.events)
            {
               await ProcessOneNotification(notificationEvent, rtu, rtuAddresses);
            }

            return true;
        }

        private async Task ProcessOneNotification(VeexNotificationEvent notificationEvent, Rtu rtu, DoubleAddress rtuAddresses)
        {
            notificationEvent.time = TimeZoneInfo.ConvertTime(notificationEvent.time, TimeZoneInfo.Local);
            _logFile.AppendLine($"test {notificationEvent.data.testId} - {notificationEvent.type} at {notificationEvent.time}");

            // Alex promised to return port in notification
            var port = 3;
            var trace = _writeModel.Traces.FirstOrDefault(t => t.RtuId == rtu.Id && t.Port == port);
            if (trace == null)
                return;  // no such a trace
            // Alex promised to return test name (to extract base ref type) in notification
            var testName = "Port 3, fast, created 26-May-21 09:59:57";
            var baseRefType = GetBaseRefTypeFromName(testName);

            // first we need to check if monitoring result should be fetched and saved
            if (!ShouldMoniResultBeSaved(notificationEvent, rtu, trace, baseRefType)) return;

            // second: fetch moni result
            var res = await _d2RtuVeexLayer3.GetMoniResult(rtuAddresses, notificationEvent);
            if (res.MeasurementResult != MeasurementResult.Success) return;

            var baseRef = await GetBaseRefSorBytes(trace, baseRefType);
        }

        private bool ShouldMoniResultBeSaved(VeexNotificationEvent notificationEvent, Rtu rtu, Trace trace, BaseRefType baseRefType)
        {
             var traceLastMeas = _writeModel.Measurements.LastOrDefault(m => m.TraceId == trace.TraceId);
            if (traceLastMeas == null) 
                return true; // first measurement on trace
            if (IsTimeToSave(notificationEvent, rtu, traceLastMeas, baseRefType))
                return true;
         
            return IsTraceStateChanged();
        }

        private bool IsTraceStateChanged()
        {

            return true;
        }

        private bool IsTimeToSave(VeexNotificationEvent notificationEvent, Rtu rtu, Measurement traceLastMeas, BaseRefType baseRefType)
        {
            var frequency = baseRefType == BaseRefType.Fast ? rtu.FastSave : rtu.PreciseSave;
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
