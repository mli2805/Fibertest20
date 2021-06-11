using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<MonitoringResultDto> GetTestLastMeasurement(DoubleAddress rtuAddresses, VeexNotificationEvent notificationEvent)
        {
            try
            {
                return await _d2RtuVeexLayer2
                    .GetTestLastMeasurement(rtuAddresses, notificationEvent.Data.TestId, notificationEvent.Type);
            }
            catch (Exception)
            {
                return null;
            }

          
        }
    }
}
