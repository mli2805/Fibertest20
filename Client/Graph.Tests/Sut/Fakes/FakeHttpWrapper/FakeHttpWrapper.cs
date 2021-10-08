using System;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Graph.Tests
{
    public class FakeHttpWrapper : IHttpWrapper
    {
        public FakeVeexRtuModel FakeVeexRtuModel { get; set; } = new FakeVeexRtuModel();

        public Task<HttpRequestResult> GetByteArray(DoubleAddress rtuDoubleAddress, string relativeUri)
        {
            return Task.FromResult(new HttpRequestResult()
            {
                HttpStatusCode = HttpStatusCode.OK,
                ResponseBytesArray = new byte[0],
            });
        }

        public Task<HttpRequestResult> PostByteArray(DoubleAddress rtuDoubleAddress, string relativeUri, byte[] bytes, byte[] bytes2 = null)
        {
            return Task.FromResult(new HttpRequestResult()
            {
                HttpStatusCode = HttpStatusCode.Created,
            });

        }

        public Task<HttpRequestResult> RequestByUrl(DoubleAddress rtuDoubleAddress, string relativeUri, string httpMethod,
            string contentRepresentation = null, string jsonData = null)
        {
            var result = new HttpRequestResult();
            switch (relativeUri.Request(httpMethod))
            {
                case "GetMonitoringProperties":
                    result.HttpStatusCode = HttpStatusCode.OK;
                    result.ResponseJson = JsonConvert.SerializeObject(new VeexMonitoringDto());
                    break;
                case "SetMonitoringProperty":
                case "ChangeProxyMode":
                case "SwitchOtauToPort":
                    result.HttpStatusCode = HttpStatusCode.NoContent;
                    break;
                case "DoMeasurementRequest":
                    result.HttpStatusCode = HttpStatusCode.Created;
                    result.ResponseJson = Guid.NewGuid().ToString();
                    break;
                case "GetMeasurementResult":
                    result.HttpStatusCode = HttpStatusCode.OK;
                    result.ResponseJson = JsonConvert.SerializeObject(new VeexMeasurementResult());
                    break;
                case "GetOtdrs":
                    result.HttpStatusCode = HttpStatusCode.OK;
                    result.ResponseJson = JsonConvert.SerializeObject(FakeVeexRtuModel.OtdrItems);
                    break;
                case "GetOtdr": //TODO select by id
                    result.HttpStatusCode = HttpStatusCode.OK;
                    result.ResponseJson = JsonConvert.SerializeObject(FakeVeexRtuModel.Otdrs[0]);
                    break;
                case "GetPlatform":
                    result.HttpStatusCode = HttpStatusCode.OK;
                    result.ResponseJson = JsonConvert.SerializeObject(FakeVeexRtuModel.PlatformInfo);
                    break;

                case "CreateOtau":
                    if (jsonData == null) break;
                    result.ResponseJson = FakeVeexRtuModel.AddOtau(JsonConvert.DeserializeObject<NewOtau>(jsonData));
                    result.HttpStatusCode = HttpStatusCode.Created;
                    break;
                case "DeleteOtau":
                    FakeVeexRtuModel.DeleteOtau(relativeUri);
                    result.HttpStatusCode = HttpStatusCode.NoContent;
                    break;
                case "ChangeOtauCascadingScheme":
                    if (jsonData == null) break;
                    FakeVeexRtuModel.Scheme = JsonConvert.DeserializeObject<VeexOtauCascadingScheme>(jsonData);
                    result.HttpStatusCode = HttpStatusCode.NoContent;
                    break;
                case "GetOtauCascadingScheme":
                    result.HttpStatusCode = HttpStatusCode.OK;
                    result.ResponseJson = JsonConvert.SerializeObject(FakeVeexRtuModel.Scheme);
                    break;
                case "GetOtau": 
                    result.HttpStatusCode = HttpStatusCode.OK;
                    result.ResponseJson = JsonConvert.SerializeObject(FakeVeexRtuModel.GetOtau(relativeUri));
                    break;
                case "GetOtaus":
                    result.HttpStatusCode = HttpStatusCode.OK;
                    result.ResponseJson = JsonConvert.SerializeObject(FakeVeexRtuModel.OtauItems);
                    break;

                case "GetTests":
                    result.HttpStatusCode = HttpStatusCode.OK;
                    result.ResponseJson = JsonConvert.SerializeObject(FakeVeexRtuModel.TestItems);
                    break;
                case "GetTest":
                    result.HttpStatusCode = HttpStatusCode.OK;
                    result.ResponseJson = JsonConvert.SerializeObject(FakeVeexRtuModel.GetTestByUri(relativeUri));
                    break;
                case "CreateTest":
                    if (jsonData == null) break;
                    result.ResponseJson = FakeVeexRtuModel.AddTest(JsonConvert.DeserializeObject<Test>(jsonData));
                    result.HttpStatusCode = HttpStatusCode.Created;
                    break;
                case "AddTestsRelation":
                    if (jsonData == null) break;
                    result.ResponseJson = FakeVeexRtuModel
                        .AddTestRelation(JsonConvert.DeserializeObject<TestsRelation>(jsonData));
                    result.HttpStatusCode = HttpStatusCode.Created;
                    break;
                case "DeleteRelation":
                    var parts = relativeUri.Split('/');
                    FakeVeexRtuModel.TestsRelations.RemoveAll(r => r.id == parts[2]);
                    result.HttpStatusCode = HttpStatusCode.NoContent;
                    break;
                case "ChangeTest":
                    result.HttpStatusCode = HttpStatusCode.NoContent;
                    break;
                case "DeleteTest":
                    result.HttpStatusCode = HttpStatusCode.NoContent;
                    break;

                case "SetBaseRef":
                    result.HttpStatusCode = HttpStatusCode.Created;
                    result.ResponseJson = Guid.NewGuid().ToString();
                    break;
                case "GetTestThresholds":
                    result.HttpStatusCode = HttpStatusCode.OK;
                    result.ResponseJson = JsonConvert.SerializeObject(new ThresholdSet());
                    break;
                case "SetThresholds":
                    result.HttpStatusCode = HttpStatusCode.Created;
                    result.ResponseJson = Guid.NewGuid().ToString();
                    break;
            }

            return Task.FromResult(result);
        }


    }
}