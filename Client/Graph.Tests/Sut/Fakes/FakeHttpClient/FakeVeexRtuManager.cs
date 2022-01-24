using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Graph.Tests
{
    public class FakeVeexRtuManager
    {
        public FakeVeexRtuModel FakeVeexRtuModel { get; set; }

        public FakeVeexRtuManager(FakeVeexRtuModel fakeVeexRtuModel)
        {
            FakeVeexRtuModel = fakeVeexRtuModel;
        }

        public Task<HttpRequestResult> GetByteArray(DoubleAddress rtuDoubleAddress, string relativeUri)
        {
            return Task.FromResult(new HttpRequestResult()
            {
                HttpStatusCode = HttpStatusCode.OK,
                ResponseBytesArray = FakeVeexRtuModel.SorBytesToReturn,
            });
        }

        public Task<HttpResponseMessage> PostByteArray()
        {
            return Task.FromResult(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent("")
            });

        }

        public Task<HttpResponseMessage> RequestByUrl(string relativeUri, string httpMethod,
            string contentRepresentation = null, string jsonData = null)
        {
            var result = new HttpResponseMessage() { Content = new StringContent("") };
            var baseUri = new Uri("http://fibertest.com/");

            switch (relativeUri.Request(httpMethod))
            {
                case "GetMonitoringProperties":
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(new VeexMonitoringDto()));
                    break;
                case "SetVesionSettings":
                case "SetMonitoringProperty":
                case "ChangeProxyMode":
                    result.StatusCode = HttpStatusCode.NoContent;
                    break;
                case "DisableVesionIntegration":
                    result.StatusCode = HttpStatusCode.NoContent;
                    break;
                case "SwitchOtauToPort":
                    result.StatusCode = FakeVeexRtuModel.SwitchOtauToPort(relativeUri, jsonData);
                    break;
                case "DoMeasurementRequest":
                    result.StatusCode = HttpStatusCode.Created;
                    FakeVeexRtuModel.MeasurementRequestId = Guid.NewGuid();
                    FakeVeexRtuModel.SorBytesToReturn = new byte[32000];
                    result.Content = new StringContent(FakeVeexRtuModel.MeasurementRequestId.ToString());
                    break;
                case "GetMeasurementResult":
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(new VeexMeasurementResult()
                    {
                        id = FakeVeexRtuModel.MeasurementRequestId.ToString(),
                        status = "finished"
                    }));
                    break;
                case "ResetOtdr":
                    result.StatusCode = HttpStatusCode.Created;
                    // result.Headers.Location = new Uri($"otdr_reconnection_requests/{ Guid.NewGuid() }");
                    var uri2 = new Uri("http://fibertest.com/"+$"otdr_reconnection_requests/{ Guid.NewGuid() }");
                    result.Headers.Location = baseUri.MakeRelativeUri(uri2);
                    break;
                case "GetResetOtdrStatus":
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(new OtdrResetResponse() { status = "processed" }));
                    break;
                case "GetOtdrs":
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(FakeVeexRtuModel.OtdrItems));
                    break;
                case "GetOtdr": //TODO select by id
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(FakeVeexRtuModel.Otdrs[0]));
                    break;
                case "GetPlatform":
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(FakeVeexRtuModel.GetPlatformInfo()));
                    break;

                case "CreateOtau":
                    if (jsonData == null) break;
                    result.StatusCode = HttpStatusCode.Created;
                    var otauLink = FakeVeexRtuModel.AddOtau(JsonConvert.DeserializeObject<VeexOtau>(jsonData));
                    var fullOtauLink = new Uri(baseUri + otauLink);
                    result.Headers.Location = baseUri.MakeRelativeUri(fullOtauLink);
                    break;
                case "DeleteOtau":
                    FakeVeexRtuModel.DeleteOtau(relativeUri);
                    result.StatusCode = HttpStatusCode.NoContent;
                    break;
                case "ChangeOtauCascadingScheme":
                    if (jsonData == null) break;
                    FakeVeexRtuModel.Scheme = JsonConvert.DeserializeObject<VeexOtauCascadingScheme>(jsonData);
                    result.StatusCode = HttpStatusCode.NoContent;
                    break;
                case "GetOtauCascadingScheme":
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(FakeVeexRtuModel.Scheme));
                    break;
                case "GetOtau":
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(FakeVeexRtuModel.GetOtau(relativeUri)));
                    break;
                case "GetOtaus":
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(FakeVeexRtuModel.OtauItems));
                    break;

                case "GetTests":
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(FakeVeexRtuModel.TestItems));
                    break;
                case "GetTest":
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(FakeVeexRtuModel.GetTestByUri(relativeUri)));
                    break;
                case "CreateTest":
                    if (jsonData == null) break;
                    var testLink = FakeVeexRtuModel.AddTest(JsonConvert.DeserializeObject<Test>(jsonData));
                    var fullTestLink = new Uri(baseUri + testLink);
                    result.Headers.Location = baseUri.MakeRelativeUri(fullTestLink); 
                    result.StatusCode = HttpStatusCode.Created;
                    break;
                case "AddTestsRelation":
                    if (jsonData == null) break;
                    var testRelationLink = FakeVeexRtuModel.AddTestRelation(JsonConvert.DeserializeObject<TestsRelation>(jsonData));
                    var fullTestRelationLink = new Uri(baseUri + testRelationLink);
                    result.Headers.Location = baseUri.MakeRelativeUri(fullTestRelationLink);
                    result.StatusCode = HttpStatusCode.Created;
                    break;
                case "DeleteRelation":
                    var parts = relativeUri.Split('/');
                    FakeVeexRtuModel.TestsRelations.RemoveAll(r => r.id == parts[2]);
                    result.StatusCode = HttpStatusCode.NoContent;
                    break;
                case "ChangeTest":
                    result.StatusCode = HttpStatusCode.NoContent;
                    break;
                case "DeleteTest":
                    result.StatusCode = HttpStatusCode.NoContent;
                    break;
                case "SetBaseRef":
                    result.StatusCode = HttpStatusCode.Created;
                    result.Headers.Location = new Uri(Guid.NewGuid().ToString());
                    break;

                case "GetCompletedTestsAfterTimestamp":
                    result.StatusCode = HttpStatusCode.OK;
                    result.Content = new StringContent(JsonConvert.SerializeObject(FakeVeexRtuModel.GetCompletedTestsAfterTimestamp(relativeUri)));
                    break;
            }

            return Task.FromResult(result);
        }


    }
}