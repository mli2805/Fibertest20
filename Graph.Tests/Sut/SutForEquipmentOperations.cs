using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SutForEquipmentOperations : SystemUnderTest
    {
        private Guid _rtuNodeId;

        public Iit.Fibertest.Graph.Trace SetTraceFromRtuThrouhgAtoB(out Guid nodeAId, out Guid equipmentA1Id, out Guid nodeBId, out Guid equipmentB1Id)
        {
            SetNodeWithEquipment(out nodeAId, out equipmentA1Id);
            SetNodeWithEquipment(out nodeBId, out equipmentB1Id);
            SetRtuAndFibers(nodeAId, nodeBId);
            return SeTrace(nodeBId);
        }

        private void SetNodeWithEquipment(out Guid nodeA, out Guid eqA)
        {
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Sleeve }).Wait();
            Poller.Tick();
            nodeA = ReadModel.Nodes.Last().Id;
            eqA = ReadModel.Equipments.Last().Id;
        }

        private void SetRtuAndFibers(Guid nodeAId, Guid nodeBId)
        {
            ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation()).Wait();
            Poller.Tick();
            _rtuNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _rtuNodeId, Node2 = nodeAId }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeAId, Node2 = nodeBId }).Wait();
            Poller.Tick();
        }

        private Iit.Fibertest.Graph.Trace SeTrace(Guid lastNodeId)
        {
            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, @"some title", "", Answer.Yes));

            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = lastNodeId, NodeWithRtuId = _rtuNodeId });
            Poller.Tick();
            return ReadModel.Traces.Last();
        }
    }
}