using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SutForTraceAdded : SystemUnderTest
    {
        public void CreateFieldForPathFinderTest(out Guid startId, out Guid finishId, out Guid wrongNodeId, out Guid wrongNodeWithEqId)
        {
            ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            Poller.Tick();
            startId = ReadModel.Rtus.Last().NodeId;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var b0 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var b1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var b2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var c0 = ReadModel.Nodes.Last().Id; wrongNodeId = c0;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var c1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Sleeve }).Wait(); var c2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var d0 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var d1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Sleeve }).Wait(); var d2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var e0 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var e1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var e2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait(); Poller.Tick(); finishId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var zz = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait(); Poller.Tick(); var z2 = ReadModel.Nodes.Last().Id;
            wrongNodeWithEqId = z2;


            ShellVm.ComplyWithRequest(new AddFiber() { Id = Guid.NewGuid(), Node1 = startId, Node2 = b0 }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = startId, Node2 = b1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = startId, Node2 = b2 }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c0, Node2 = b0 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c1, Node2 = b1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c2, Node2 = b2 }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c0, Node2 = d0 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c1, Node2 = d1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c2, Node2 = d2 }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = e0, Node2 = d0 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = e1, Node2 = d1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = e2, Node2 = d2 }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = e2, Node2 = finishId }).Wait();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = zz, Node2 = z2 }).Wait();
            Poller.Tick();
        }

    }
}