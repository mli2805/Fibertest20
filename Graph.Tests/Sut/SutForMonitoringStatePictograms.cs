using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public class SutForMonitoringStatePictograms : SutForTraceAttach
    {
        public List<Iit.Fibertest.Graph.Trace> CreateThreeTracesRtuEmptyTerminal()
        {
            var result = new List<Iit.Fibertest.Graph.Trace>();

            ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            Poller.EventSourcingTick().Wait();
            var nodeForRtuId = ReadModel.Rtus.Last().NodeId;

            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode }).Wait();
            Poller.EventSourcingTick().Wait();
            var middleNodeId = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = middleNodeId }).Wait();

            for (int i = 0; i < 3; i++)
            {
                ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
                Poller.EventSourcingTick().Wait();
                var endNodeId = ReadModel.Nodes.Last().Id;

                ShellVm.ComplyWithRequest(new AddFiber() { Node1 = middleNodeId, Node2 = endNodeId }).Wait();
                Poller.EventSourcingTick().Wait();
                result.Add(DefineTrace(endNodeId, nodeForRtuId));
            }
            return result;
        }
    }
}