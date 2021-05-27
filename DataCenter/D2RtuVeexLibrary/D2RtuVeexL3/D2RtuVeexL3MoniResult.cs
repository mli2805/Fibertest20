using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<MonitoringResultDto> GetMoniResult(DoubleAddress rtuAddresses, VeexNotificationEvent notificationEvent)
        {
            try
            {
                var result = await _d2RtuVeexLayer2.GetCompletedTest(rtuAddresses, notificationEvent.data.testId,
                    notificationEvent.type);
                if (result == null)
                    return null;
                return new MonitoringResultDto();
            }
            catch (Exception)
            {
                return null;
            }

          
        }
    }
}
