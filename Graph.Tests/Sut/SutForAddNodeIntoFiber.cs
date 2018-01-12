using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public class SutForAddNodeIntoFiber : SutForBaseRefs
    {
        public void CreatePositionForAddNodeIntoFiberTest(out Iit.Fibertest.Graph.Fiber fiberForInsertion,
            out Iit.Fibertest.Graph.Trace traceForInsertionId)
        {
            ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            Poller.EventSourcingTick().Wait();
            var nodeForRtuId = ReadModel.Rtus.Last().NodeId;
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            Poller.EventSourcingTick().Wait();
            var a1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            Poller.EventSourcingTick().Wait();
            var b1 = ReadModel.Nodes.Last().Id;
            // fiber for insertion
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = a1, Node2 = b1 }).Wait();
            Poller.EventSourcingTick().Wait();
            fiberForInsertion = ShellVm.ReadModel.Fibers.Last();

            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.EventSourcingTick().Wait();
            var a2 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.EventSourcingTick().Wait();
            var b2 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.EventSourcingTick().Wait();
            var c2 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.EventSourcingTick().Wait();
            var d2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = a1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = a1, Node2 = a2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = b1, Node2 = b2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = b1, Node2 = c2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = d2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = b1, Node2 = a2 }).Wait();
            Poller.EventSourcingTick().Wait();

            traceForInsertionId = DefineTrace(a2, nodeForRtuId);
            DefineTrace(b2, nodeForRtuId);
            DefineTrace(c2, nodeForRtuId);
            DefineTrace(d2, nodeForRtuId);
        }

    }
}