using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SutForEquipmentAdded : SutForEquipment
    {
        private Guid _rtuNodeId, _anotherNodeId, _anotherNodeId2;
        public Guid OldEquipmentId;
        public Guid NodeId;
        public Guid ShortTraceId, TraceWithEqId, TraceWithoutEqId;

        public void SetNode()
        {
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Sleeve }).Wait();
            Poller.Tick();
            NodeId = ReadModel.Nodes.Last().Id;
            OldEquipmentId = ReadModel.Equipments.Last().Id;
        }

        public void SetRtuAndOthers()
        {
            ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation()).Wait();
            Poller.Tick();
            _rtuNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _rtuNodeId, Node2 = NodeId }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            _anotherNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _anotherNodeId, Node2 = NodeId }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Other }).Wait();
            Poller.Tick();
            _anotherNodeId2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _anotherNodeId2, Node2 = NodeId }).Wait();
            Poller.Tick();
        }

        public void SetShortTrace()
        {
            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(
                model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(
                model => AddTraceViewHandler(model, @"short trace", "", Answer.Yes));

            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = NodeId, NodeWithRtuId = _rtuNodeId });
            Poller.Tick();
            ShortTraceId = ReadModel.Traces.Last().Id;
        }

        public void SetLongTraceWithEquipment()
        {
            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(
                model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(
                model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(
                model => AddTraceViewHandler(model, @"trace with eq", "", Answer.Yes));

            ShellVm.ComplyWithRequest(new RequestAddTrace()
            {
                LastNodeId = _anotherNodeId,
                NodeWithRtuId = _rtuNodeId
            });
            Poller.Tick();
            TraceWithEqId = ReadModel.Traces.Last().Id;
        }

        public void SetLongTraceWithoutEquipment()
        {
            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(
                model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 1));
            FakeWindowManager.RegisterHandler(
                model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(
                model => AddTraceViewHandler(model, @"trace without eq", "", Answer.Yes));

            ShellVm.ComplyWithRequest(new RequestAddTrace()
            {
                LastNodeId = _anotherNodeId2,
                NodeWithRtuId = _rtuNodeId
            });
            Poller.Tick();
            TraceWithoutEqId = ReadModel.Traces.Last().Id;
        }

    }
}