﻿using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Graph.Tests
{
    public class FakeVeexRtuModel
    {
        public VeexPlatformInfo PlatformInfo { get; set; } = new VeexPlatformInfo();
        public LinkList OtdrItems { get; set; }
        public List<VeexOtdr> Otdrs { get; set; }
        public LinkList OtauItems { get; set; }
        public List<VeexOtau> Otaus { get; set; }
        public VeexOtauCascadingScheme Scheme { get; set; }

        public LinkList TestItems { get; set; }
        public Dictionary<string, Test> Tests { get; set; } = new Dictionary<string, Test>();
        public List<TestsRelation> TestsRelations { get; set; } = new List<TestsRelation>();

        public FakeVeexRtuModel()
        {
            var otdr = new VeexOtdr()
            {
                id = Guid.NewGuid().ToString(),
                supportedMeasurementParameters = new SupportedMeasurementParameters()
                {
                    laserUnits = new Dictionary<string, LaserUnit>()
                    {
                        {
                            @"SM1625",
                            new LaserUnit() {distanceRanges = new Dictionary<string, DistanceRange>()}
                        }
                    }
                }
            };
            Otdrs = new List<VeexOtdr>() { otdr };
            OtdrItems = new LinkList { items = new List<LinkObject>() { new LinkObject() { self = $@"otdrs/{otdr.id}" } }, total = 1 };

            var mainOtau = new VeexOtau()
            {
                id = @"S1_OXA-4000__32",
                protocol = @"db25",
                inputPortCount = 1,
                portCount = 32,
                serialNumber = @"123456789"
            };
            OtauItems = new LinkList() { items = new List<LinkObject>() { new LinkObject() { self = $@"otaus/{mainOtau.id}" } }, total = 1 };
            Otaus = new List<VeexOtau>() { mainOtau };

            Scheme = new VeexOtauCascadingScheme()
            {
                rootConnections = new List<RootConnection>() { new RootConnection() { inputOtauId = mainOtau.id, inputOtauPort = 0 } },
                connections = new List<Connection>(),
            };

            TestItems = new LinkList() { items = new List<LinkObject>() };
        }

        public string AddTest(Test test)
        {
            test.relations = new RelationItems() {items = new List<TestsRelation>()};
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
            OtauItems.items.Add(new LinkObject(){self = link});
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

    }
}
