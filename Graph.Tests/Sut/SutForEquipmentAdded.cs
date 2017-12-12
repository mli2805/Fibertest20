using System;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

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
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure }).Wait();
            Poller.EventSourcingTick().Wait();
            NodeId = ReadModel.Nodes.Last().Id;
            OldEquipmentId = ReadModel.Equipments.Last().Id;
        }

        public void SetRtuAndOthers()
        {
            ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation()).Wait();
            Poller.EventSourcingTick().Wait();
            _rtuNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _rtuNodeId, Node2 = NodeId }).Wait();
            Poller.EventSourcingTick().Wait();

            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.EventSourcingTick().Wait();
            _anotherNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _anotherNodeId, Node2 = NodeId }).Wait();
            Poller.EventSourcingTick().Wait();

            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Other }).Wait();
            Poller.EventSourcingTick().Wait();
            _anotherNodeId2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _anotherNodeId2, Node2 = NodeId }).Wait();
            Poller.EventSourcingTick().Wait();
        }

        public void SetShortTrace()
        {
            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(
                model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(
                model => AddTraceViewHandler(model, @"short trace", "", Answer.Yes));

            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = NodeId, NodeWithRtuId = _rtuNodeId });
            Poller.EventSourcingTick().Wait();
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
            Poller.EventSourcingTick().Wait();
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
            Poller.EventSourcingTick().Wait();
            TraceWithoutEqId = ReadModel.Traces.Last().Id;
        }

        public void SetThreeTraceThroughNode()
        {
            SetNode();
            SetRtuAndOthers();
            SetShortTrace();
            SetLongTraceWithEquipment();
            SetLongTraceWithoutEquipment();
        }

    }
}