using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Graph.Tests
{
    public static class FakeVeexRtuModelExt
    {
        public static VeexPlatformInfo GetPlatformInfo(this FakeVeexRtuModel fakeVeexRtuModel)
        {
            return new VeexPlatformInfo()
            {
                platform = new Platform()
                {
                    serialNumber = fakeVeexRtuModel.Omsn
                }
            };
        }
        public static string AddTest(this FakeVeexRtuModel fakeVeexRtuModel, Test test)
        {
            test.relations = new RelationItems() { items = new List<TestsRelation>() };
            fakeVeexRtuModel.Tests.Add(test.id, test);
            var link = $@"tests/{test.id}";
            fakeVeexRtuModel.TestItems.items.Add(new LinkObject() { self = link });
            fakeVeexRtuModel.TestItems.total++;
            return link;
        }

        public static Test GetTestByUri(this FakeVeexRtuModel fakeVeexRtuModel, string uri)
        {
            var pos = uri.LastIndexOf('/');
            var pos2 = uri.IndexOf('?');
            var id = uri.Substring(pos + 1, pos2 - pos - 1);
            return fakeVeexRtuModel.Tests[id];
        }

        public static string AddTestRelation(this FakeVeexRtuModel fakeVeexRtuModel, TestsRelation relation)
        {
            fakeVeexRtuModel.Tests[relation.testAId].relations.items.Add(relation);
            fakeVeexRtuModel.Tests[relation.testBId].relations.items.Add(relation);
            fakeVeexRtuModel.TestsRelations.Add(relation);
            return $@"test_relations/{relation.id}";
        }

        public static string AddOtau(this FakeVeexRtuModel fakeVeexRtuModel, VeexOtau otau)
        {
            var link = $@"otaus/{otau.id}";
            fakeVeexRtuModel.OtauItems.items.Add(new LinkObject() { self = link });

            if (otau.portCount == 0) otau.portCount = 16;
            if (otau.protocol == "") otau.protocol = "tcpip";
            if (otau.serialNumber == "") otau.serialNumber = "12345678";

            fakeVeexRtuModel.Otaus.Add(otau);
            return link;
        }

        public static VeexOtau GetOtau(this FakeVeexRtuModel fakeVeexRtuModel, string link)
        {
            var parts = link.Split('/');
            return fakeVeexRtuModel.Otaus.First(o => o.id == parts[1]);
        }

        public static void DeleteOtau(this FakeVeexRtuModel fakeVeexRtuModel, string link)
        {
            var parts = link.Split('/');
            fakeVeexRtuModel.Otaus.RemoveAll(o => o.id == parts[1]);
            fakeVeexRtuModel.OtauItems.items.RemoveAll(i => i.self == link);
            var conn = fakeVeexRtuModel.Scheme.connections.FirstOrDefault(c => c.inputOtauId == parts[1]);
            if (conn != null)
                fakeVeexRtuModel.Scheme.connections.Remove(conn);
        }

        public static CompletedTestPortion GetCompletedTestsAfterTimestamp(this FakeVeexRtuModel fakeVeexRtuModel, string uri)
        {
            var pos = "monitoring/completed?fields=*,items.*&starting=".Length;
            var pos2 = uri.LastIndexOf('&');
            var str = uri.Substring(pos + 1, pos2 - pos - 1);
            var timestamp = DateTime.Parse(str);
            var completedTests = fakeVeexRtuModel.CompletedTests.Where(c => c.started > timestamp).ToList();
            return new CompletedTestPortion()
            {
                items = completedTests,
                total = completedTests.Count,
            };
        }

        public static HttpStatusCode SwitchOtauToPort(this FakeVeexRtuModel fakeVeexRtuModel, string link, string jsonData)
        {
            var parts = link.Split('/');
            var otau = fakeVeexRtuModel.Otaus.First(o => o.id == parts[1]); // test crashes if otau not found

            dynamic myObj = JsonConvert.DeserializeObject<dynamic>(jsonData);
            int portIndex = myObj?.portIndex ?? -1;

            return portIndex > 0 && portIndex <= otau.portCount
                ? HttpStatusCode.NoContent
                : HttpStatusCode.BadRequest;
        }
    }
}