using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        public async Task<HttpRequestResult> GetTests(DoubleAddress rtuDoubleAddress)
        {
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress, "monitoring/tests", "get");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            if (res.IsSuccessful)
                res.ResponseObject = JsonConvert.DeserializeObject<LinkList>(res.ResponseJson);
            return res;  
        }
      
        public async Task<HttpRequestResult> GetTest(DoubleAddress rtuDoubleAddress, string testLink)
        {
            var relativeUri = $@"monitoring/{testLink}?fields=*,otauPorts.*,relations.items.*";
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress, relativeUri, "get");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            if (res.IsSuccessful)
                res.ResponseObject = JsonConvert.DeserializeObject<Test>(res.ResponseJson);
            return res;
        }

        public async Task<HttpRequestResult> CreateTest(DoubleAddress rtuDoubleAddress, Test test)
        {
            var content = JsonConvert.SerializeObject(test, IgnoreNulls);
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                "monitoring/tests", "post", "application/json", content);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.Created;
            return res;
        }

        public async Task<HttpRequestResult> AddTestsRelation(DoubleAddress rtuDoubleAddress, TestsRelation relation)
        {
            var content = JsonConvert.SerializeObject(relation);
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                "monitoring/test_relations", "post", "application/json", content);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.Created;
            return res;
        }

        public async Task<HttpRequestResult> DeleteRelation(DoubleAddress rtuDoubleAddress, string relationId)
        {
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress, 
                $@"monitoring/test_relations/{relationId}", "delete");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.NoContent;
            return res;
        }


        public async Task<HttpRequestResult> ChangeTest(DoubleAddress rtuDoubleAddress, string testLink, Test test)
        {
            var jsonData = JsonConvert.SerializeObject(test, IgnoreNulls);
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                $@"monitoring/{testLink}", "patch", "application/merge-patch+json", jsonData);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.NoContent;
            return res;
        }

        public async Task<HttpRequestResult> DeleteTest(DoubleAddress rtuDoubleAddress, string testLink)
        {
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress, $@"monitoring/{testLink}", "delete");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.NoContent;
            return res;
        }

        public async Task<HttpRequestResult> SetBaseRef(DoubleAddress rtuDoubleAddress, string testLink, byte[] sorBytes, byte[] sorBytes2 = null)
        {
            var res = await _httpWrapper.PostByteArray(
                rtuDoubleAddress, $@"monitoring/{testLink}/references", sorBytes, sorBytes2);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.Created;
            return res;
        }
    }
}
