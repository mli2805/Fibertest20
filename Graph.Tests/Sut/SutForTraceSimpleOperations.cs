using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SutForTraceSimpleOperations : SystemUnderTest
    {
        public Iit.Fibertest.Graph.Trace CreateSimpleTraceRtuEmptyTerminal()
        {
            ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            Poller.Tick();
            var nodeForRtuId = ReadModel.Rtus.Last().NodeId;

            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            Poller.Tick();
            var firstNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            var secondNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = firstNodeId }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = firstNodeId, Node2 = secondNodeId }).Wait();
            Poller.Tick();

            return DefineTrace(secondNodeId, nodeForRtuId);
        }

        protected Iit.Fibertest.Graph.Trace DefineSimpleTrace(Guid lastNodeId, Guid nodeForRtuId)
        {
            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, @"some title", "", Answer.Yes));
            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = lastNodeId, NodeWithRtuId = nodeForRtuId });
            Poller.Tick();
            return ShellVm.ReadModel.Traces.Last();
        }

        public void InitializeRtu(Guid rtuId, int portCount)
        {
            ShellVm.ComplyWithRequest(new InitializeRtu()
            {
                Id = rtuId,
                FullPortCount = portCount,
                OwnPortCount = portCount,
                MainChannel = new NetAddress(),
                ReserveChannel = new NetAddress(),
            }).Wait();
            Poller.Tick();
        }
        public void AttachTrace(Guid traceId)
        {
            var traceLeaf = ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(traceId);
            FakeWindowManager.RegisterHandler(model => TraceToAttachHandler(model, Answer.Yes));
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