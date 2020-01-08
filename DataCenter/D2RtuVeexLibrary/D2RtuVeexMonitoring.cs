using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class D2RtuVeexMonitoring
    {
        private readonly HttpExt _httpExt;

        public D2RtuVeexMonitoring(HttpExt httpExt)
        {
            _httpExt = httpExt;
        }

        /// <summary>
        /// start / stop monitoring
        /// </summary>
        /// <param name="rtuDoubleAddress"></param>
        /// <param name="mode">enabled (Auto) or disabled (Manual)</param>
        /// <returns></returns>
        public async Task<HttpRequestResult> SetMonitoringMode(DoubleAddress rtuDoubleAddress, string mode)
        {
            var json = JsonConvert.SerializeObject(new MonitoringVeexDto() { state = mode });
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress,
                "monitoring", "patch", "application/merge-patch+json", json);
            return httpResult;
        }

        public async Task<bool> GetMonitoringMode(DoubleAddress rtuDoubleAddress)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, "monitoring", "get");
            if (httpResult.HttpStatusCode == HttpStatusCode.OK)
            {
                var j = JObject.Parse(httpResult.ResponseJson);
                var rr = (string)j["state"];
                return (rr == "enabled");
            }

            return false;
        }

        public async Task<Tests> GetTests(DoubleAddress rtuDoubleAddress)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, "monitoring/tests", "get");
            return httpResult.HttpStatusCode == HttpStatusCode.OK
                ? JsonConvert.DeserializeObject<Tests>(httpResult.ResponseJson)
                : null;
        }

      
        public async Task<HttpRequestResult> CreateTest(DoubleAddress rtuDoubleAddress, CreateTestCmd test)
        {
            var content = JsonConvert.SerializeObject(test);
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                "monitoring/tests", "post", "application/json", content);
        }

        public async Task<Test> GetTest(DoubleAddress rtuDoubleAddress, string testUri)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, testUri, "get");
            return httpResult.HttpStatusCode == HttpStatusCode.OK
                ? JsonConvert.DeserializeObject<Test>(httpResult.ResponseJson)
                : null;
        }

        private static readonly JsonSerializerSettings ignoreNulls = new JsonSerializerSettings(){ NullValueHandling = NullValueHandling.Ignore };
        public async Task<HttpRequestResult> ChangeTest(DoubleAddress rtuDoubleAddress, string testUri, Test test)
        {
            var jsonData = JsonConvert.SerializeObject(test, ignoreNulls);
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                testUri, "patch", "application/merge-patch+json", jsonData);
        }



        public async Task<bool> DeleteTest(DoubleAddress rtuDoubleAddress, string testUri)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, testUri, "delete");
            return httpResult.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task<ThresholdSet> GetTestThresholds(DoubleAddress rtuDoubleAddress, string setUri)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, setUri, "get");
            return httpResult.HttpStatusCode == HttpStatusCode.OK
                ? JsonConvert.DeserializeObject<ThresholdSet>(httpResult.ResponseJson)
                : null;
        }

        public async Task<bool> SetBaseRef(DoubleAddress rtuDoubleAddress, string refUri, byte[] sorBytes)
        {
            var httpResult = await _httpExt.PostFile(rtuDoubleAddress, refUri, sorBytes);
            return httpResult.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> SetThresholds(DoubleAddress rtuDoubleAddress, string thresholdUri)
        {
            var thresholdSet = new ThresholdSet()
            {
                levels = new List<Level>()
                {
                    new Level()
                    {
                        groups = new List<Group>()
                        {
                            new Group()
                            {
                                thresholds = new Thresholds()
                                {
                                    eventLeadingLossCoefficient = new CombinedThreshold(){decrease = 1},
                                    eventLoss = new CombinedThreshold(){decrease = 2, increase = 2},
                                    eventReflectance = new CombinedThreshold(){min = 4, max = 4}
                                }
                            }
                        }, name = "minor"
                    },
                    new Level()
                    {
                        groups = new List<Group>(), name = "major"
                    },
                    new Level()
                    {
                        groups = new List<Group>(), name = "critical"
                    },
                }
            };
            var jsonData = JsonConvert.SerializeObject(thresholdSet);
            var httpResult = await _httpExt.RequestByUrl(
                rtuDoubleAddress, thresholdUri, "post", "application/json", jsonData);
            return httpResult.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> GetFile(DoubleAddress rtuDoubleAddress, string fileUri)
        {
            var httpResult = await _httpExt.GetFile(rtuDoubleAddress, fileUri);
            return httpResult.HttpStatusCode == HttpStatusCode.OK;

        }

    }
}