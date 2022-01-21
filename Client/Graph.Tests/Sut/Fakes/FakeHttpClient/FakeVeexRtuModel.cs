using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Graph.Tests
{
    public class FakeVeexRtuModel
    {
        public Guid Id { get; set; }
        public RtuMaker RtuMaker { get; set; }
        public string Mfsn { get; set; }
        public string Omsn { get; set; }

        // public VeexPlatformInfo PlatformInfo { get; set; } = new VeexPlatformInfo();
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

        public void Initialize()
        {
            Id = Guid.NewGuid();
            RtuMaker = RtuMaker.VeEX;
            Omsn = "1105618";

            var otdr = new VeexOtdr()
            {
                id = Guid.NewGuid().ToString(),
                isConnected = true,
                supportedMeasurementParameters = new SupportedMeasurementParameters()
                {
                    laserUnits = new Dictionary<string, LaserUnit>()
                    {
                        {
                            "SM1625",
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
    
    }
}
