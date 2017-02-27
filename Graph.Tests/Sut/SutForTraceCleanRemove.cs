using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SutForTraceCleanRemove : SystemUnderTest
    {
        public void CreateTwoTraces(out Guid traceId1, out Guid traceId2)
        {
            ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            Poller.Tick();
            var nodeForRtuId = ReadModel.Rtus.Last().NodeId;

            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            Poller.Tick();
            var a1 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            var a2 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            var b2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = a1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = a1, Node2 = a2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = a1, Node2 = b2 }).Wait();
            Poller.Tick();

            traceId1 = DefineTrace(a2, nodeForRtuId).Id;
            traceId2 = DefineTrace(b2, nodeForRtuId).Id;
        }

        public void AttachTrace(Guid traceId)
        {
            var traceLeaf = ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(traceId);
            FakeWindowManager.RegisterHandler(model=>TraceToAttachHandler(model, Answer.Yes));
            var rtuLeaf = traceLeaf.Parent;
            var portLeaf = (PortLeaf)rtuLeaf.Children[1]; // 2nd port
            portLeaf.AttachFromListAction(null);
            Poller.Tick();
        }

        public bool TraceToAttachHandler(object model, Answer answer)
        {
            var vm = model as TraceToAttachViewModel;
            if (vm == null) return false;
            if (answer == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }

    }
}