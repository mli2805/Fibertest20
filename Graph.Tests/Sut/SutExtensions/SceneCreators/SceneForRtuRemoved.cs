using System;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public static class SceneForRtuRemoved
    {
        public static Iit.Fibertest.Graph.Rtu CreateRtuA(this SystemUnderTest sut)
        {
            sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation()).Wait();
            sut.Poller.EventSourcingTick().Wait();
            return sut.ReadModel.Rtus.Last();
        }

        public static Guid[] CreateOneRtuAndFewNodesAndFibers(this SystemUnderTest sut, Guid rtuANodeId)
        {
            var result = new Guid[3];

            sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation()).Wait();
            sut.Poller.EventSourcingTick().Wait();
            result[0] = sut.ReadModel.Rtus.Last().NodeId;

            sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var node1Id = sut.ReadModel.Nodes.Last().Id;

            sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            result[1] = sut.ReadModel.Nodes.Last().Id;
            sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            result[2] = sut.ReadModel.Nodes.Last().Id;

            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = rtuANodeId, Node2 = node1Id }).Wait();
            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = node1Id, Node2 = result[1] }).Wait();
            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = result[1], Node2 = result[2] }).Wait();
            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = result[2], Node2 = result[0] }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            return result;
        }

       
    }
}