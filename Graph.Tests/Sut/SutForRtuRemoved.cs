using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SutForRtuRemoved : SutForTraceAttach
    {
        public Guid RtuANodeId, RtuBNodeId;
        private Guid _node2Id, _node3Id;
        public Guid Fiber1Id, Fiber2Id, Fiber3Id, Fiber4Id;

       

        public Guid CreateRtu()
        {
            ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation()).Wait();
            Poller.Tick();
            RtuANodeId = ReadModel.Rtus.Last().NodeId;
            return ReadModel.Rtus.Last().Id;
        }
        public void CreateOneRtuAndFewNodesAndFibers()
        {
            ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation()).Wait();
            Poller.Tick();
            RtuBNodeId = ReadModel.Rtus.Last().NodeId;

            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            Poller.Tick();
            var node1Id = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            _node2Id = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            _node3Id = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = RtuANodeId, Node2 = node1Id }).Wait();
            Poller.Tick();
            Fiber1Id = ReadModel.Fibers.Last().Id;
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = node1Id, Node2 = _node2Id }).Wait();
            Poller.Tick();
            Fiber2Id = ReadModel.Fibers.Last().Id;
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _node2Id, Node2 = _node3Id }).Wait();
            Poller.Tick();
            Fiber3Id = ReadModel.Fibers.Last().Id;
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _node3Id, Node2 = RtuBNodeId }).Wait();
            Poller.Tick();
            Fiber4Id = ReadModel.Fibers.Last().Id;
        }

        public Guid CreateTrace()
        {
            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, @"some trace title", "", Answer.Yes));

            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _node3Id, NodeWithRtuId = RtuANodeId });
            Poller.Tick();
            return ReadModel.Traces.Last().Id;
        }

        public void CreateAnotherTraceWhichInterceptedFirst()
        {
            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, @"some trace title", "", Answer.Yes));

            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _node2Id, NodeWithRtuId = RtuBNodeId });
            Poller.Tick();
        }
    }
}