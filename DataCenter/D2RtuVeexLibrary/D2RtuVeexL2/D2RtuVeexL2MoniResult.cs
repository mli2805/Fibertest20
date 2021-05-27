using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<MonitoringResultDto> GetCompletedTest(DoubleAddress rtuAddresses, string testId, string type)
        {
            var kind = type == "monitoring_test_passed" ? "last_passed" : "last_failed";
            try
            {
                var completedTest = await _d2RtuVeexLayer1.GetCompletedTest(rtuAddresses, testId, kind);
                if (completedTest == null) return null;

                var sorBytes = await _d2RtuVeexLayer1.GetSorBytes(rtuAddresses, testId, kind);
                return new MonitoringResultDto()
                {
                    SorBytes = sorBytes,
                };
            }
            catch (Exception)
            {
                return null;
            }

          
        }
    }
}
