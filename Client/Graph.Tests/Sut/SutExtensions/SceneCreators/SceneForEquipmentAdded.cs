using System;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;

namespace Graph.Tests
{
    public static class SceneForEquipmentAdded
    {

        public static Iit.Fibertest.Graph.Equipment SetNode(this SystemUnderTest sut)
        {
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var nodeId = sut.ReadModel.Nodes.Last().NodeId;
            return sut.ReadModel.Equipments.First(e => e.NodeId == nodeId && e.Type == EquipmentType.Closure);
        }

        public static Iit.Fibertest.Graph.Rtu SetRtuAndOthers(this SystemUnderTest sut, Guid nodeId, out Guid anotherNodeId, out Guid anotherNodeId2)
        {
            sut.FakeWindowManager.RegisterHandler(model => sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation());
            sut.Poller.EventSourcingTick().Wait();
            var rtuNodeId = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = rtuNodeId, NodeId2 = nodeId }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            anotherNodeId = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = anotherNodeId, NodeId2 = nodeId }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Other }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            anotherNodeId2 = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = anotherNodeId2, NodeId2 = nodeId }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            return sut.ReadModel.Rtus.Last();
        }

        public static Guid SetShortTrace(this SystemUnderTest sut, Guid nodeId, Guid rtuNodeId)
        {
            sut.FakeWindowManager.RegisterHandler(sut.WaitFormHandler);
            sut.FakeWindowManager.RegisterHandler(model => sut.OneLineMessageBoxAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            sut.FakeWindowManager.RegisterHandler(model => sut.TraceContentChoiceHandler(model, Answer.Yes, 0));
            sut.FakeWindowManager.RegisterHandler(
                model => sut.AddTraceViewHandler(model, @"short trace", "", Answer.Yes));

            sut.GraphReadModel.AddTrace(new RequestAddTrace() { LastNodeId = nodeId, NodeWithRtuId = rtuNodeId }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            return sut.ReadModel.Traces.Last().TraceId;
        }

        public static Guid SetLongTraceWithEquipment(this SystemUnderTest sut, Guid rtuNodeId, Guid anotherNodeId)
        {
            sut.FakeWindowManager.RegisterHandler(sut.WaitFormHandler);
            sut.FakeWindowManager.RegisterHandler(model => sut.OneLineMessageBoxAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            sut.FakeWindowManager.RegisterHandler(model => sut.TraceContentChoiceHandler(model, Answer.Yes, 0));
            sut.FakeWindowManager.RegisterHandler(model => sut.TraceContentChoiceHandler(model, Answer.Yes, 0));
            sut.FakeWindowManager.RegisterHandler(
                model => sut.AddTraceViewHandler(model, @"trace with eq", "", Answer.Yes));

            sut.GraphReadModel.AddTrace(new RequestAddTrace()
            {
                LastNodeId = anotherNodeId,
                NodeWithRtuId = rtuNodeId
            }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            return sut.ReadModel.Traces.Last().TraceId;
        }

        public static Guid SetLongTraceWithoutEquipment(this SystemUnderTest sut, Guid rtuNodeId, Guid anotherNodeId2)
        {
            sut.FakeWindowManager.RegisterHandler(sut.WaitFormHandler);
            sut.FakeWindowManager.RegisterHandler(model => sut.OneLineMessageBoxAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            sut.FakeWindowManager.RegisterHandler(model => sut.TraceContentChoiceHandler(model, Answer.Yes, 1));
            sut.FakeWindowManager.RegisterHandler(model => sut.TraceContentChoiceHandler(model, Answer.Yes, 0));
            sut.FakeWindowManager.RegisterHandler(
                model => sut.AddTraceViewHandler(model, @"trace without eq", "", Answer.Yes));

            sut.GraphReadModel.AddTrace(new RequestAddTrace()
            {
                LastNodeId = anotherNodeId2,
                NodeWithRtuId = rtuNodeId
            }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            return sut.ReadModel.Traces.Last().TraceId;
        }

        public static void SetThreeTraceThroughNode(this SystemUnderTest sut, 
            out Iit.Fibertest.Graph.Equipment oldEquipment, out Guid shortTraceId, out Guid traceWithEqId, out Guid traceWithoutEqId)
        {
            oldEquipment = sut.SetNode();
            var rtuNodeId = sut.SetRtuAndOthers(oldEquipment.NodeId, out var anotherNodeId, out var anotherNodeId2).NodeId;
            shortTraceId = sut.SetShortTrace(oldEquipment.NodeId, rtuNodeId);
            traceWithEqId = sut.SetLongTraceWithEquipment(rtuNodeId, anotherNodeId);
            traceWithoutEqId = sut.SetLongTraceWithoutEquipment(rtuNodeId, anotherNodeId2);
        }

    }
}