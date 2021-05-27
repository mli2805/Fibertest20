using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceWebC2D
    {
        public async Task<bool> MonitoringMeasurementDone(VeexMeasurementDto veexMeasurementDto)
        {
            await Task.Delay(3000);
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == veexMeasurementDto.RtuId);
            if (rtu == null) return false;
            var rtuAddresses = new DoubleAddress()
                {Main = rtu.MainChannel, HasReserveAddress = rtu.IsReserveChannelSet, Reserve = rtu.ReserveChannel};


            foreach (var notificationEvent in veexMeasurementDto.VeexNotification.events)
            {
                var localTime = TimeZoneInfo.ConvertTime(notificationEvent.time, TimeZoneInfo.Local);
                _logFile.AppendLine($"test {notificationEvent.data.testId} - {notificationEvent.type} at {localTime}");

                // first we need to check if monitoring result should be fetched


                // second fetch moni result
                var res = await _d2RtuVeexLayer3.GetMoniResult(rtuAddresses, notificationEvent);
                if (res.MeasurementResult == MeasurementResult.Success)
                    _logFile.AppendLine("Hooray!");
            }

            return true;
        }
    }
}
