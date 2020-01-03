﻿using System;
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

        public async Task<bool> CreateTest(DoubleAddress rtuDoubleAddress, Test test)
        {
            var content = JsonConvert.SerializeObject(test);
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, 
                "monitoring/tests", "post", "application/json", content);
            return httpResult.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task<Test> GetTest(DoubleAddress rtuDoubleAddress, string testUri)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, testUri, "get");
            return httpResult.HttpStatusCode == HttpStatusCode.OK
                ? JsonConvert.DeserializeObject<Test>(httpResult.ResponseJson)
                : null;
        }

        public async Task<bool> ChangeTest(DoubleAddress rtuDoubleAddress, string testUri)
        {
            // var content = new Dictionary<string, int> {{"period", 1000}};
            var content1 = new Dictionary<string, string> { { "state", "disabled" } };
            var jsonData = JsonConvert.SerializeObject(content1);
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, 
                testUri, "patch", "application/merge-patch+json", jsonData);
            return httpResult.HttpStatusCode == HttpStatusCode.OK;
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

    }
}