﻿using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        public async Task<CompletedTest> GetCompletedTest(DoubleAddress rtuDoubleAddress, string testId, string kind)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, $"monitoring/tests/{testId}/completed/{kind}", "get");
            return httpResult.HttpStatusCode == HttpStatusCode.OK
                ? JsonConvert.DeserializeObject<CompletedTest>(httpResult.ResponseJson)
                : null;
        }

        public async Task<byte[]> GetSorBytes(DoubleAddress rtuDoubleAddress,  string testId, string kind)
        {
            var httpResult = await _httpExt.GetByteArray(rtuDoubleAddress, $"monitoring/tests/{testId}/completed/{kind}/traces/0.sor");
            return httpResult.HttpStatusCode == HttpStatusCode.OK
                ? httpResult.ResponseBytesArray
                : null;
        }

    }
}
