using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;

namespace Graph.Tests
{
    public static class SceneForBaseRefAdjuster
    {
        public static Iit.Fibertest.Graph.Rtu SetInitializedRtu(this SystemUnderTest sut)
        {
            sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var rtu = sut.ReadModel.Rtus.Last();

            sut.InitializeRtu(rtu.Id, @"1.1.1.1", "", @"SM1550");
            return rtu;
        }

        public static Iit.Fibertest.Graph.Trace SetTrace(this SystemUnderTest sut, Guid rtuNodeId, string title)
        {
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Cross, Latitude = 55.002, Longitude = 30.002 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var nodeIdA = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Node1 = rtuNodeId, Node2 = nodeIdA }).Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55.02, Longitude = 30.02 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var nodeIdB = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55.04, Longitude = 30.04 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            nodeIdA = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55.06, Longitude = 30.06 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            nodeIdB = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure, Latitude = 55.08, Longitude = 30.08 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            nodeIdA = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55.082, Longitude = 30.082 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            nodeIdB = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55.084, Longitude = 30.084 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            nodeIdA = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal, Latitude = 55.086, Longitude = 30.086 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            nodeIdB = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            return sut.DefineTrace(nodeIdB, rtuNodeId, title, 8);
        }

        public static void AddAdjustmentPoints(this SystemUnderTest sut, Iit.Fibertest.Graph.Trace trace)
        {
            var fibers = sut.ReadModel.GetTraceFibers(trace).ToArray();
            sut.ShellVm.GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber(){FiberId = fibers[1].Id, InjectionType = EquipmentType.AdjustmentPoint, Position = new PointLatLng(55.01,30.01)}).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var nodeOfPointId = sut.ReadModel.Nodes.Last().Id;

            sut.GraphReadModel.GrmNodeRequests.MoveNode(new MoveNode() { NodeId = nodeOfPointId, Latitude = 55.0086, Longitude = 30.0114}).Wait();
            sut.Poller.EventSourcingTick().Wait();

            sut.ShellVm.GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber(){FiberId = fibers[2].Id, InjectionType = EquipmentType.AdjustmentPoint, Position = new PointLatLng(55.0306,30.0298)}).Wait();
            sut.Poller.EventSourcingTick().Wait();
            nodeOfPointId = sut.ReadModel.Nodes.Last().Id;

            sut.GraphReadModel.GrmNodeRequests.MoveNode(new MoveNode() { NodeId = nodeOfPointId, Latitude = 55.0326, Longitude = 30.0314}).Wait();
            sut.Poller.EventSourcingTick().Wait();
        }

        public static void AddCableReserve(this SystemUnderTest sut, Iit.Fibertest.Graph.Trace trace)
        {
            sut.FakeWindowManager.RegisterHandler(model => sut.TraceChoiceHandler(model, new List<Guid>() { trace.Id }, Answer.Yes));
            sut.FakeWindowManager.RegisterHandler(model => sut.CableReserveInfoViewModelHandler(model, Answer.Yes, 100));
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentIntoNode(new RequestAddEquipmentIntoNode() { NodeId = trace.Nodes[5], IsCableReserveRequested = true}).Wait();
            sut.Poller.EventSourcingTick().Wait();
        }

        public static void AddEquipmentWithCableReserve(this SystemUnderTest sut, Iit.Fibertest.Graph.Trace trace)
        {
            sut.FakeWindowManager.RegisterHandler(model => sut.TraceChoiceHandler(model, new List<Guid>() { trace.Id }, Answer.Yes));
            sut.FakeWindowManager.RegisterHandler(model => sut.EquipmentInfoViewModelHandler(model, Answer.Yes, EquipmentType.CableReserve, 40, 30));
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentIntoNode(new RequestAddEquipmentIntoNode() { NodeId = trace.Nodes[5], IsCableReserveRequested = false}).Wait();
            sut.Poller.EventSourcingTick().Wait();
        }
    }

}
