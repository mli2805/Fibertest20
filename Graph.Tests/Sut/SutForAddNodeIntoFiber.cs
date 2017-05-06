using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public class SutForAddNodeIntoFiber : SystemUnderTest
    {
        public void CreatePositionForAddNodeIntoFiberTest(out Iit.Fibertest.Graph.Fiber fiberForInsertion, out Iit.Fibertest.Graph.Trace traceForInsertionId)
        {
            ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            Poller.Tick();
            var nodeForRtuId = ReadModel.Rtus.Last().NodeId;
            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            Poller.Tick();
            var a1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            Poller.Tick();
            var b1 = ReadModel.Nodes.Last().Id;
            // fiber for insertion
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = a1, Node2 = b1 }).Wait();
            Poller.Tick();
            fiberForInsertion = ShellVm.ReadModel.Fibers.Last();

            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            var a2 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            var b2 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            var c2 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            var d2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = a1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = a1, Node2 = a2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = b1, Node2 = b2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = b1, Node2 = c2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = d2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = b1, Node2 = a2 }).Wait();
            Poller.Tick();

            traceForInsertionId = DefineTrace(a2, nodeForRtuId);
            DefineTrace(b2, nodeForRtuId);
            DefineTrace(c2, nodeForRtuId);
            DefineTrace(d2, nodeForRtuId);
        }

    }
}