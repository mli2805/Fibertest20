using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SutForEquipmentAdded : SutForEquipment
    {
        public Guid _nodeId, _rtuNodeId, _anotherNodeId, _anotherNodeId2;
        public Guid _shortTraceId, _traceWithEqId, _traceWithoutEqId;
        public Guid _oldEquipmentId;

        public void SetNode()
        {
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Sleeve }).Wait();
            Poller.Tick();
            _nodeId = ReadModel.Nodes.Last().Id;
            _oldEquipmentId = ReadModel.Equipments.Last().Id;
        }

        public void SetRtuAndOthers()
        {
            ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation()).Wait();
            Poller.Tick();
            _rtuNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _rtuNodeId, Node2 = _nodeId }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.Tick();
            _anotherNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _anotherNodeId, Node2 = _nodeId }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Other }).Wait();
            Poller.Tick();
            _anotherNodeId2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _anotherNodeId2, Node2 = _nodeId }).Wait();
            Poller.Tick();
        }

        public void SetShortTrace()
        {
            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(
                model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(
                model => AddTraceViewHandler(model, @"short trace", "", Answer.Yes));

            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _nodeId, NodeWithRtuId = _rtuNodeId });
            Poller.Tick();
            _shortTraceId = ReadModel.Traces.Last().Id;
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
            _traceWithEqId = ReadModel.Traces.Last().Id;
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
            _traceWithoutEqId = ReadModel.Traces.Last().Id;
        }

    }
}