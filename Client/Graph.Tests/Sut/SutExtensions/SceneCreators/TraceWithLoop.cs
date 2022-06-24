using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public static class TraceWithLoop
    {
        public static Iit.Fibertest.Graph.Trace CreateTraceDoublePassingClosure(this SystemUnderTest sut, string title = @"some title")
        {
            sut.FakeWindowManager.RegisterHandler(model => sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            sut.Poller.EventSourcingTick().Wait();
            var nodeForRtuId = sut.ReadModel.Rtus.Last().NodeId;
            var rtuId = sut.ReadModel.Rtus.Last().Id;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure, }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var closureNodeId = sut.ReadModel.Nodes.Last().NodeId;
            var closureId = sut.ReadModel.Equipments.Last(e => e.Type == EquipmentType.Closure).EquipmentId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Cross, }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var crossNodeId = sut.ReadModel.Nodes.Last().NodeId;
            var crossId = sut.ReadModel.Equipments.Last(e => e.Type == EquipmentType.Cross).EquipmentId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal, }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var terminalNodeId = sut.ReadModel.Nodes.Last().NodeId;
            var terminalId = sut.ReadModel.Equipments.Last(e => e.Type == EquipmentType.Terminal).EquipmentId;

            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = nodeForRtuId, NodeId2 = closureNodeId }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var fiber = sut.ReadModel.Fibers.Last();
            sut.GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber() { FiberId = fiber.FiberId, InjectionType = EquipmentType.AdjustmentPoint }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var adjustmentNodeId1 = sut.ReadModel.Nodes.Last().NodeId;
            var adjustmentEquipmentId1 = sut.ReadModel.Equipments.Last(e => e.Type == EquipmentType.AdjustmentPoint).EquipmentId;


            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = closureNodeId, NodeId2 = crossNodeId }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            fiber = sut.ReadModel.Fibers.Last();
            sut.GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber() { FiberId = fiber.FiberId, InjectionType = EquipmentType.EmptyNode }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var emptyNodeId1 = sut.ReadModel.Nodes.Last().NodeId;
            var emptyEquipmentId1 = sut.ReadModel.Equipments.Last(e => e.Type == EquipmentType.AdjustmentPoint).EquipmentId;

            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = closureNodeId, NodeId2 = terminalNodeId }).Wait();
            sut.Poller.EventSourcingTick().Wait();
        
            var traceNodes = new List<Guid>() { nodeForRtuId, adjustmentNodeId1, closureNodeId, emptyNodeId1, crossNodeId, emptyNodeId1, closureNodeId, terminalNodeId };
            var traceEquipments = new List<Guid>() { rtuId, adjustmentEquipmentId1, closureId, emptyEquipmentId1, crossId, emptyEquipmentId1, closureId, terminalId };

            var traceAddViewModel = sut.ClientScope.Resolve<TraceInfoViewModel>();
            traceAddViewModel.Initialize(Guid.NewGuid(), traceEquipments, traceNodes, true).Wait();
            traceAddViewModel.Title = title;
            traceAddViewModel.Save();
            sut.Poller.EventSourcingTick().Wait();

            return sut.ReadModel.Traces.Last();
        }
    }
}