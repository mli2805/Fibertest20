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


        public async Task<HttpRequestResult> GetCompletedTestsAfterTimestamp(DoubleAddress rtuDoubleAddress,
            string timestamp, int limit)
        {
            return await _d2RtuVeexLayer2.GetCompletedTestsAfterTimestamp(rtuDoubleAddress, timestamp, limit);
        }

        public async Task<HttpRequestResult> GetCompletedTestSorBytes(DoubleAddress rtuDoubleAddress, string measId)
        {
            return await _d2RtuVeexLayer2.GetCompletedTestSorBytes(rtuDoubleAddress, measId);
        }

    }
}
