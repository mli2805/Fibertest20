using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public static class SceneBasicTraceRtuEmptyTerminal
    {
        public static Iit.Fibertest.Graph.Trace CreateTraceRtuEmptyTerminal(this SystemUnderTest sut, string title = @"some title")
        {
            sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var nodeForRtuId = sut.ReadModel.Rtus.Last().NodeId;

            sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation()
            {
                Type = EquipmentType.EmptyNode,
                Latitude = 55.1,
                Longitude = 30.1
            }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var firstNodeId = sut.ReadModel.Nodes.Last().Id;

            sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation()
            {
                Type = EquipmentType.Terminal,
                Latitude = 55.2,
                Longitude = 30.2
            }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var secondNodeId = sut.ReadModel.Nodes.Last().Id;

            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = firstNodeId }).Wait();
            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = firstNodeId, Node2 = secondNodeId }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            return sut.DefineTrace(secondNodeId, nodeForRtuId, title);
        }
    }
}