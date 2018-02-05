using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;

namespace Graph.Tests
{
    public static class SceneForMonitoringStatePictograms
    {
        public static List<Iit.Fibertest.Graph.Trace> CreateThreeTracesRtuEmptyTerminal(this SystemUnderTest sut)
        {
            var result = new List<Iit.Fibertest.Graph.Trace>();

            sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var nodeForRtuId = sut.ReadModel.Rtus.Last().NodeId;

            sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var middleNodeId = sut.ReadModel.Nodes.Last().Id;
            sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = middleNodeId }).Wait();

            for (int i = 0; i < 3; i++)
            {
                sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
                sut.Poller.EventSourcingTick().Wait();
                var endNodeId = sut.ReadModel.Nodes.Last().Id;

                sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = middleNodeId, Node2 = endNodeId }).Wait();
                sut.Poller.EventSourcingTick().Wait();
                result.Add(sut.DefineTrace(endNodeId, nodeForRtuId));
            }
            return result;
        }
    }
}