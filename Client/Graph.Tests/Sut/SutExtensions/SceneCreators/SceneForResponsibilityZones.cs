using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public static class SceneForResponsibilityZones
    {
        public static Iit.Fibertest.Graph.Rtu SetInitializedRtuForZone1(this SystemUnderTest sut)
        {
            sut.FakeWindowManager.RegisterHandler(model => sut.RtuUpdateHandler(model, @"RTU for zone 1", @"doesn't matter", Answer.Yes));
            sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 56, Longitude = 31 });
            sut.Poller.EventSourcingTick().Wait();
            var rtu = sut.ReadModel.Rtus.Last();

            sut.FakeD2RWcfManager.SetFakeInitializationAnswer(waveLength: @"SM1550");
            sut.SetNameAndAskInitializationRtu(rtu.Id, @"2.2.2.2");
            return rtu;
        }

        public static Iit.Fibertest.Graph.Trace SetTraceForZone(this SystemUnderTest sut, Guid rtuNodeId, string title)
        {
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var terminalNodeId = sut.ReadModel.Nodes.Last().NodeId;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = rtuNodeId, NodeId2 = terminalNodeId }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            return sut.DefineTrace(terminalNodeId, rtuNodeId, title, 1);
        }

    }
}