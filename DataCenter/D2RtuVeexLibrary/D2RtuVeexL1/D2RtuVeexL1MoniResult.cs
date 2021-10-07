using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        public async Task<HttpRequestResult> GetCompletedTest(DoubleAddress rtuDoubleAddress, string testId, string kind)
        {
            var res = await _httpWrapper.RequestByUrl(
                rtuDoubleAddress, $"monitoring/tests/{testId}/completed/{kind}", "get");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            res.ResponseObject = JsonConvert.DeserializeObject<CompletedTest>(res.ResponseJson);
            return res;
        }

        public async Task<HttpRequestResult> GetSorBytes(DoubleAddress rtuDoubleAddress, string testId, string kind)
        {
            var res = await _httpWrapper.GetByteArray(
                rtuDoubleAddress, $"monitoring/tests/{testId}/completed/{kind}/traces/0.sor");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            return res;
        }

        public async Task<HttpRequestResult> GetCompletedTestsAfterTimestamp(DoubleAddress rtuDoubleAddress, string timestamp, int limit)
        {
            var res = await _httpWrapper.RequestByUrl(
                rtuDoubleAddress, $"monitoring/completed?fields=*,items.*&starting={timestamp}&limit={limit}", "get");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            if (res.IsSuccessful)
                res.ResponseObject = JsonConvert.DeserializeObject<CompletedTestPortion>(res.ResponseJson);
            return res;
        }

        public async Task<HttpRequestResult> GetCompletedTestSorBytes(DoubleAddress rtuDoubleAddress, string measId)
        {
            var res = await _httpWrapper.GetByteArray(
                rtuDoubleAddress, $"monitoring/completed/{measId}/traces/0.sor");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            return res;
        }
    }
}
