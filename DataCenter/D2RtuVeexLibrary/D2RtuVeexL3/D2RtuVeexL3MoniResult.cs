using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<MonitoringResultDto> GetTestLastMeasurement(DoubleAddress rtuAddresses, VeexNotificationEvent notificationEvent, bool isFast)
        {
            try
            {
                return await _d2RtuVeexLayer2
                    .GetTestLastMeasurement(rtuAddresses, notificationEvent.data.testId, notificationEvent.type, isFast);
            }
            catch (Exception)
            {
                return null;
            }

          
        }
    }
}
