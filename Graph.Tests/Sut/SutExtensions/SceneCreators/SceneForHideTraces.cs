using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;

namespace Graph.Tests
{
    public class SceneForHideTraces : SystemUnderTest
    {
        public Guid EmptyNode1Id, EmptyNode2Id;
        public Guid CommonClosureNodeId { get; set; }
        public Guid CommonTerminalNodeId { get; set; }

        public Guid NodeAId, NodeBId, NodeCId;
        public Iit.Fibertest.Graph.Rtu Rtu1, Rtu2;
        public Iit.Fibertest.Graph.Trace Trace1 { get; set; }
        public Iit.Fibertest.Graph.Trace Trace2 { get; set; }


        public void CreateRtu1WithTrace1()
        {
            FakeWindowManager.RegisterHandler(model => this.RtuUpdateHandler(model, @"RTU2", @"doesn't matter", Answer.Yes));
            GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            Poller.EventSourcingTick().Wait();
            Rtu1 = ReadModel.Rtus.Last();

            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.EmptyNode }).Wait();
            Poller.EventSourcingTick().Wait();
            EmptyNode1Id = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.Closure }).Wait();
            Poller.EventSourcingTick().Wait();
            CommonClosureNodeId = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.Terminal }).Wait();
            Poller.EventSourcingTick().Wait();
            CommonTerminalNodeId = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = Rtu1.NodeId, NodeId2 = EmptyNode1Id }).Wait();
            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = EmptyNode1Id, NodeId2 = CommonClosureNodeId }).Wait();
            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = CommonClosureNodeId, NodeId2 = CommonTerminalNodeId }).Wait();
            Poller.EventSourcingTick().Wait();

            Trace1 = this.DefineTrace(CommonTerminalNodeId, Rtu1.NodeId, @"trace1", 3);
        }

        public void CreateRtu2WithTrace2PartlyOverlappingTrace1()
        {
            FakeWindowManager.RegisterHandler(model => this.RtuUpdateHandler(model, @"RTU1", @"doesn't matter", Answer.Yes));
            GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            Poller.EventSourcingTick().Wait();
            Rtu2 = ReadModel.Rtus.Last();

            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.EmptyNode }).Wait();
            Poller.EventSourcingTick().Wait();
            EmptyNode2Id = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = Rtu2.NodeId, NodeId2 = EmptyNode2Id }).Wait();
            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = EmptyNode2Id, NodeId2 = CommonClosureNodeId }).Wait();
            Poller.EventSourcingTick().Wait();

            Trace2 = this.DefineTrace(CommonTerminalNodeId, Rtu2.NodeId, @"trace2", 3);
        }

        public void CreateFiberToRtu2()
        {
            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.Other }).Wait();
            Poller.EventSourcingTick().Wait();
            NodeAId = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = Rtu2.NodeId, NodeId2 = NodeAId }).Wait();
        }

        public void CreateFiberBtoC()
        {
            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.Other }).Wait();
            Poller.EventSourcingTick().Wait();
            NodeBId = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation { Type = EquipmentType.Other }).Wait();
            Poller.EventSourcingTick().Wait();
            NodeCId = ReadModel.Nodes.Last().NodeId;

            GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = NodeBId, NodeId2 = NodeCId }).Wait();
        }
    }
}
