using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentRemovedSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _nodeId, _rtuNodeId, _anotherNodeId;
        private Guid _equipmentId;
        private Guid _traceId;


        [Given(@"Существует узел с оборудованием")]
        public void GivenСуществуетУзелСОборудованием()
        {
            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Sleeve}).Wait();
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
            _equipmentId = _sut.ReadModel.Equipments.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation()).Wait();
            _sut.Poller.Tick();
            _rtuNodeId = _sut.ReadModel.Nodes.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _rtuNodeId, Node2 = _nodeId }).Wait();
            _sut.Poller.Tick();

            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            _sut.Poller.Tick();
            _anotherNodeId = _sut.ReadModel.Nodes.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _anotherNodeId, Node2 = _nodeId }).Wait();
            _sut.Poller.Tick();
        }

        [Given(@"Трасса проходит в узле но не использует данное оборудование")]
        public void GivenТрассаПроходитВУзлеНоНеИспользуетДанноеОборудование()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 1));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, @"some title", "", Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _anotherNodeId, NodeWithRtuId = _rtuNodeId }).Wait();
            _sut.Poller.Tick();
            _traceId = _sut.ReadModel.Traces.Last().Id;
        }

        [Given(@"Существует трасса c данным оборудованием в середине")]
        public void GivenСуществуетТрассаCДаннымОборудованиемВСередине()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, @"some title", "", Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _anotherNodeId, NodeWithRtuId = _rtuNodeId }).Wait();
            _sut.Poller.Tick();
            _traceId = _sut.ReadModel.Traces.Last().Id;
        }

        [Given(@"Существует трасса c данным оборудованием в конце")]
        public void GivenСуществуетТрассаCДаннымОборудованиемВКонце()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, @"some title", "", Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _nodeId, NodeWithRtuId = _rtuNodeId }).Wait();
            _sut.Poller.Tick();
        }


        [Given(@"Для этой трассы задана базовая")]
        public void GivenДляЭтойТрассыЗаданаБазовая()
        {
            _sut.FakeWindowManager.BaseIsSet();
            _sut.ShellVm.ComplyWithRequest(new RequestAssignBaseRef() { TraceId = _traceId }).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Пункт Удалить недоступен для данного оборудования")]
        public void ThenПунктУдалитьНедоступенДляДанногоОборудования()
        {
            var vm = new NodeUpdateViewModel(_nodeId, _sut.ShellVm.GraphVm, _sut.FakeWindowManager);
            vm.EquipmentsInNode.First(e => e.Id == _equipmentId).IsRemoveEnabled.Should().BeFalse();
        }

        [When(@"Пользователь нажимает удалить оборудование")]
        public void WhenПользовательНажимаетУдалитьОборудование()
        {
            // не  работает , т.к. некому реагировать на RemoveEquipment
            // надо начинать с ShellVM
            var vm = new NodeUpdateViewModel(_nodeId, _sut.ShellVm.GraphVm, _sut.FakeWindowManager);
            vm.RemoveEquipment(new RemoveEquipment() {Id = _equipmentId});
            vm.Cancel();
            _sut.Poller.Tick();
        }

        [Then(@"Оборудование удаляется из трассы")]
        public void ThenОборудованиеУдаляетсяИзТрассы()
        {
            _sut.ReadModel.Traces.Where(t => t.Equipments.Contains(_equipmentId)).Should().BeEmpty();

        }

        [Then(@"Оборудование удаляется")]
        public void ThenОборудованиеУдаляется()
        {
            _sut.ReadModel.Equipments.FirstOrDefault(e=>e.Id == _equipmentId).Should().BeNull();
        }

        [Then(@"Оборудование НЕ удаляется")]
        public void ThenОборудованиеНеУдаляется()
        {
            _sut.ReadModel.Traces.Where(t => t.Equipments.Contains(_equipmentId)).Should().NotBeEmpty();
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == _equipmentId).Should().NotBeNull();
        }
    }
}
