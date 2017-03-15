using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SutForRtuRemoved : SutForTraceAttach
    {
        public Guid RtuNodeId;
        private Guid _endTraceNodeId;
        public Guid TraceId;

        public void CreateRtuAndFewNodesAndFibers()
        {
            ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation()).Wait();
            Poller.Tick();
            var rtu = ReadModel.Rtus.Last();
            RtuNodeId = rtu.NodeId;

            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            Poller.Tick();
            var n1 = ReadModel.Nodes.Last();
            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            Poller.Tick();
            var n2 = ReadModel.Nodes.Last();
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            _endTraceNodeId = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = rtu.NodeId, Node2 = n1.Id }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = n1.Id, Node2 = _endTraceNodeId }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = rtu.NodeId, Node2 = n2.Id }).Wait();
            Poller.Tick();
        }

        public void CreateTrace()
        {
            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, @"some trace title", "", Answer.Yes));

            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _endTraceNodeId, NodeWithRtuId = RtuNodeId });
            Poller.Tick();
            TraceId = ReadModel.Traces.Last().Id;
        }
    }
}