using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        public async Task<TestsLinks> GetTests(DoubleAddress rtuDoubleAddress)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, "monitoring/tests", "get");
            return httpResult.HttpStatusCode == HttpStatusCode.OK
                ? JsonConvert.DeserializeObject<TestsLinks>(httpResult.ResponseJson)
                : null;
        }

        public async Task<Test> GetTest(DoubleAddress rtuDoubleAddress, string testLink)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, $@"monitoring/{testLink}", "get");
            return httpResult.HttpStatusCode == HttpStatusCode.OK
                ? JsonConvert.DeserializeObject<Test>(httpResult.ResponseJson)
                : null;
        }

        public async Task<HttpRequestResult> CreateTest(DoubleAddress rtuDoubleAddress, CreateTestCmd test)
        {
            var content = JsonConvert.SerializeObject(test);
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                "monitoring/tests", "post", "application/json", content);
        }

        private static readonly JsonSerializerSettings ignoreNulls = new JsonSerializerSettings(){ NullValueHandling = NullValueHandling.Ignore };
      
        public async Task<bool> ChangeTest(DoubleAddress rtuDoubleAddress, string testLink, Test test)
        {
            var jsonData = JsonConvert.SerializeObject(test, ignoreNulls);
            var result = await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"monitoring/{testLink}", "patch", "application/merge-patch+json", jsonData);
            return result.HttpStatusCode == HttpStatusCode.NoContent;
        }

        public async Task<bool> DeleteTest(DoubleAddress rtuDoubleAddress, string testLink)
        {
            var result = await _httpExt.RequestByUrl(rtuDoubleAddress, $@"monitoring/{testLink}", "delete");
            return result.HttpStatusCode == HttpStatusCode.NoContent;
        }
    }
}
