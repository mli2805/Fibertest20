using System;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public class SutForTraceAdded : SystemUnderTest
    {
        public void CreateFieldForPathFinderTest(out Guid startId, out Guid finishId, out Guid wrongNodeId, out Guid wrongNodeWithEqId)
        {
            ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            Poller.EventSourcingTick();
            startId = ReadModel.Rtus.Last().NodeId;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.EventSourcingTick(); var b0 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.EventSourcingTick(); var b1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.EventSourcingTick(); var b2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.EventSourcingTick(); var c0 = ReadModel.Nodes.Last().Id; wrongNodeId = c0;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.EventSourcingTick(); var c1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure }).Wait(); var c2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.EventSourcingTick(); var d0 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.EventSourcingTick(); var d1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure }).Wait(); var d2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.EventSourcingTick(); var e0 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.EventSourcingTick(); var e1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.EventSourcingTick(); var e2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait(); Poller.EventSourcingTick(); finishId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.EventSourcingTick(); var zz = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait(); Poller.EventSourcingTick(); var z2 = ReadModel.Nodes.Last().Id;
            wrongNodeWithEqId = z2;


            ShellVm.ComplyWithRequest(new AddFiber() { Id = Guid.NewGuid(), Node1 = startId, Node2 = b0 }).Wait();
            Poller.EventSourcingTick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = startId, Node2 = b1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = startId, Node2 = b2 }).Wait();
            Poller.EventSourcingTick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c0, Node2 = b0 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c1, Node2 = b1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c2, Node2 = b2 }).Wait();
            Poller.EventSourcingTick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c0, Node2 = d0 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c1, Node2 = d1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c2, Node2 = d2 }).Wait();
            Poller.EventSourcingTick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = e0, Node2 = d0 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = e1, Node2 = d1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = e2, Node2 = d2 }).Wait();
            Poller.EventSourcingTick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = e2, Node2 = finishId }).Wait();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = zz, Node2 = z2 }).Wait();
            Poller.EventSourcingTick();
        }

    }
}