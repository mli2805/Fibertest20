using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;

namespace Graph.Tests
{
    public static class SceneForAddNodeIntoFiber
    {
        public static void CreatePositionForAddNodeIntoFiberTest(this SystemUnderTest sut, out Iit.Fibertest.Graph.Fiber fiberForInsertion,
            out Iit.Fibertest.Graph.Trace traceForInsertionId)
        {
            sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var nodeForRtuId = sut.ReadModel.Rtus.Last().NodeId;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var a1 = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var b1 = sut.ReadModel.Nodes.Last().Id;
            // fiber for insertion
            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = a1, Node2 = b1 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            fiberForInsertion = sut.ShellVm.ReadModel.Fibers.Last();

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var a2 = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var b2 = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var c2 = sut.ReadModel.Nodes.Last().Id;
            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var d2 = sut.ReadModel.Nodes.Last().Id;
            
            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = a1 }).Wait();
            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = a1, Node2 = a2 }).Wait();
            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = b1, Node2 = b2 }).Wait();
            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = b1, Node2 = c2 }).Wait();
            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = d2 }).Wait();
            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = b1, Node2 = a2 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            
            traceForInsertionId = sut.DefineTrace(a2, nodeForRtuId, @"some title", 3);
            sut.DefineTrace(b2, nodeForRtuId, @"some title", 3);
            sut.DefineTrace(c2, nodeForRtuId, @"some title", 3);
            sut.DefineTrace(d2, nodeForRtuId);
        }

    }
}