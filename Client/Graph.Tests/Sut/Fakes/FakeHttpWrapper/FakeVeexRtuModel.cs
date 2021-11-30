using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Graph.Tests
{
    public class FakeVeexRtuModel
    {
        public Guid Id { get; set; }
        public VeexPlatformInfo PlatformInfo { get; set; } = new VeexPlatformInfo();
        public LinkList OtdrItems { get; set; }
        public List<VeexOtdr> Otdrs { get; set; }
        public LinkList OtauItems { get; set; }
        public List<VeexOtau> Otaus { get; set; }
        public VeexOtauCascadingScheme Scheme { get; set; }

        public LinkList TestItems { get; set; }
        public Dictionary<string, Test> Tests { get; set; } = new Dictionary<string, Test>();
        public List<TestsRelation> TestsRelations { get; set; } = new List<TestsRelation>();

        public List<CompletedTest> CompletedTests { get; set; } = new List<CompletedTest>();

        public Guid MeasurementRequestId { get; set; }

        public byte[] SorBytesToReturn { get; set; }

        public FakeVeexRtuModel()
        {
            Id = Guid.NewGuid();
            Initialize(@"SM1625");
        }

        public void Initialize(string waveLength)
        {
            var otdr = new VeexOtdr()
            {
                id = Guid.NewGuid().ToString(),
                supportedMeasurementParameters = new SupportedMeasurementParameters()
                {
                    laserUnits = new Dictionary<string, LaserUnit>()
                    {
                        {
                            waveLength,
                            new LaserUnit() {
                                distanceRanges = new Dictionary<string, DistanceRange>()
                                {
                                    {
                                        "100", new DistanceRange()
                                        {
                                            averagingTimes = new []{"", ""},
                                            fastAveragingTimes = new []{"", ""},
                                            pulseDurations = new []{"", ""},
                                            resolutions = new []{"", ""},
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            };
            Otdrs = new List<VeexOtdr>() { otdr };
            OtdrItems = new LinkList
            { items = new List<LinkObject>() { new LinkObject() { self = $@"otdrs/{otdr.id}" } }, total = 1 };

            var mainOtau = new VeexOtau()
            {
                connected = true,
                connectionParameters = new VeexOtauConnectionParameters(),
                id = @"S1_OXA-4000__32",
                protocol = @"db25",
                inputPortCount = 1,
                portCount = 32,
                serialNumber = @"123456789"
            };
            OtauItems = new LinkList()
            { items = new List<LinkObject>() { new LinkObject() { self = $@"otaus/{mainOtau.id}" } }, total = 1 };
            Otaus = new List<VeexOtau>() { mainOtau };

            Scheme = new VeexOtauCascadingScheme()
            {
                rootConnections = new List<RootConnection>()
                    {new RootConnection() {inputOtauId = mainOtau.id, inputOtauPort = 0}},
                connections = new List<Connection>(),
            };

            TestItems = new LinkList() { items = new List<LinkObject>() };
        }

        public string AddTest(Test test)
        {
            test.relations = new RelationItems() { items = new List<TestsRelation>() };
            Tests.Add(test.id, test);
            var link = $@"tests/{test.id}";
            TestItems.items.Add(new LinkObject() { self = link });
            TestItems.total++;
            return link;
        }

        public Test GetTestByUri(string uri)
        {
            var pos = uri.LastIndexOf('/');
            var pos2 = uri.IndexOf('?');
            var id = uri.Substring(pos + 1, pos2 - pos - 1);
            return Tests[id];
        }

        public string AddTestRelation(TestsRelation relation)
        {
            Tests[relation.testAId].relations.items.Add(relation);
            Tests[relation.testBId].relations.items.Add(relation);
            TestsRelations.Add(relation);
            return $@"test_relations/{relation.id}";
        }

        public string AddOtau(NewOtau otau)
        {
            var link = $@"otaus/{otau.id}";
            OtauItems.items.Add(new LinkObject() { self = link });
            Otaus.Add(new VeexOtau()
            {
                id = otau.id,
                portCount = 16,
                protocol = "tcpip",
                serialNumber = "12345678",
            });
            return link;
        }

        public VeexOtau GetOtau(string link)
        {
            var parts = link.Split('/');
            return Otaus.First(o => o.id == parts[1]);
        }

        public void DeleteOtau(string link)
        {
            var parts = link.Split('/');
            Otaus.RemoveAll(o => o.id == parts[1]);
            OtauItems.items.RemoveAll(i => i.self == link);
        }

        public CompletedTestPortion GetCompletedTestsAfterTimestamp(string uri)
        {
            var pos = "monitoring/completed?fields=*,items.*&starting=".Length;
            var pos2 = uri.LastIndexOf('&');
            var str = uri.Substring(pos + 1, pos2 - pos - 1);
            var timestamp = DateTime.Parse(str);
            var completedTests = CompletedTests.Where(c => c.started > timestamp).ToList();
            return new CompletedTestPortion()
            {
                items = completedTests,
                total = completedTests.Count,
            };
        }

        public HttpStatusCode SwitchOtauToPort(string link, string jsonData)
        {
            var parts = link.Split('/');
            var otau = Otaus.First(o => o.id == parts[1]); // test crashes if otau not found

            dynamic myObj = JsonConvert.DeserializeObject<dynamic>(jsonData);
            int portIndex = myObj?.portIndex ?? -1;

            return portIndex > 0 && portIndex <= otau.portCount 
                ? HttpStatusCode.NoContent 
                : HttpStatusCode.BadRequest;
        }
    }
}
