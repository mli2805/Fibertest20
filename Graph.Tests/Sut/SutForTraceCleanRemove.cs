using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SutForTraceCleanRemove : SystemUnderTest
    {
        private Guid _nodeForRtuId, _a2, _b2;
        private void SetupNodesAndFibers()
        {
            ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            Poller.Tick();
            _nodeForRtuId = ReadModel.Rtus.Last().NodeId;

            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            Poller.Tick();
            var a1 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            _a2 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            _b2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _nodeForRtuId, Node2 = a1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = a1, Node2 = _a2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = a1, Node2 = _b2 }).Wait();
            Poller.Tick();
        }

        public void CreateTwoTraces(out Guid attachedTraceId, out Guid notAttachedTraceId)
        {
            SetupNodesAndFibers();

            var trace = DefineTrace(_a2, _nodeForRtuId);
            FakeWindowManager.RegisterHandler(model=>TraceToAttachHandler(model, Answer.Yes));
            var rtuLeaf = ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(trace.RtuId);
            var portLeaf = (PortLeaf)rtuLeaf.Children[1]; // 2nd port
            portLeaf.AttachFromListAction(null);

            attachedTraceId = trace.Id;
            notAttachedTraceId = DefineTrace(_b2, _nodeForRtuId).Id;
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