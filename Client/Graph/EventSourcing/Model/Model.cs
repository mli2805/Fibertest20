using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class Model
    {
        public License License { get; set; }
        public List<Node> Nodes { get; set; } = new List<Node>();
        public List<Fiber> Fibers { get; set; } = new List<Fiber>();
        public List<Equipment> Equipments { get; set; } = new List<Equipment>();
        public List<Rtu> Rtus { get; set; } = new List<Rtu>();
        public List<Trace> Traces { get; set; } = new List<Trace>();
        public List<Otau> Otaus { get; set; } = new List<Otau>();
        public List<User> Users { get; set; } = new List<User>();
        public List<Zone> Zones { get; set; } = new List<Zone>();
        public List<Measurement> Measurements { get; set; } = new List<Measurement>();
        public List<Measurement> ActiveMeasurements { get; set; } = new List<Measurement>(); // for all zones
        public List<NetworkEvent> NetworkEvents { get; set; } = new List<NetworkEvent>();
        public List<BopNetworkEvent> BopNetworkEvents { get; set; } = new List<BopNetworkEvent>();
        public List<BaseRef> BaseRefs { get; set; } = new List<BaseRef>();
        public List<Olt> Olts { get; set; } = new List<Olt>()
        // {
        //     new Olt()
        //     {
        //         Id = Guid.NewGuid(),
        //         Ip = @"192.168.96.21",
        //         OltModel = OltModel.Huawei_MA5608T,
        //         Relations = new Dictionary<int, Tuple<Guid, OtauPortDto>> 
        //             {{5, new Tuple<Guid, OtauPortDto>(Guid.Empty, new OtauPortDto() { OtauId = Guid.Empty.ToString(), OpticalPort = 1 })}},
        //     },
        //
        //     new Olt()
        //     {
        //         Id = Guid.NewGuid(),
        //         Ip = @"192.168.96.59",
        //         OltModel = OltModel.Huawei_MA5608T,
        //         Relations = new Dictionary<int, Tuple<Guid, OtauPortDto>> 
        //             {{5, new Tuple<Guid, OtauPortDto>(Guid.Empty, new OtauPortDto() { OtauId = Guid.Empty.ToString(), OpticalPort = 1 })}},
        //     },
        // }
        ;

        public void CopyFrom(Model source)
        {
            License = source.License;
            Nodes = source.Nodes;
            Fibers = source.Fibers;
            Equipments = source.Equipments;
            Rtus = source.Rtus;
            Traces = source.Traces;
            Otaus = source.Otaus;
            Users = source.Users;
            Zones = source.Zones;
            Measurements = source.Measurements;
            NetworkEvents = source.NetworkEvents;
            BopNetworkEvents = source.BopNetworkEvents;
            BaseRefs = source.BaseRefs;
            Olts = source.Olts;
        }
    }
}