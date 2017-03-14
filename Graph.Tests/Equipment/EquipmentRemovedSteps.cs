using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentRemovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _nodeAId, _equipmentA1Id;
        private Guid _nodeBId, _equipmentB1Id;
        private Guid _rtuNodeId;
        private Iit.Fibertest.Graph.Trace _trace;
        private NodeUpdateViewModel _vm;


        [Given(@"Существует узел A с оборудованием A1")]
        public void GivenСуществуетУзелСОборудованием()
        {
            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Sleeve}).Wait();
            _sut.Poller.Tick();
            _nodeAId = _sut.ReadModel.Nodes.Last().Id;
            _equipmentA1Id = _sut.ReadModel.Equipments.Last().Id;
        }

        [Given(@"Существует узел B с оборудованием B1")]
        public void GivenСуществуетУзелBсОборудованиемB1()
        {
            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Sleeve }).Wait();
            _sut.Poller.Tick();
            _nodeBId = _sut.ReadModel.Nodes.Last().Id;
            _equipmentB1Id = _sut.ReadModel.Equipments.Last().Id;
        }

        [Given(@"Существуют RTU и волокна")]
        public void GivenСуществуетRtuиЕщеПаруУзлов()
        {
            _sut.ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation()).Wait();
            _sut.Poller.Tick();
            _rtuNodeId = _sut.ReadModel.Nodes.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _rtuNodeId, Node2 = _nodeAId }).Wait();
            _sut.Poller.Tick();

            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _nodeAId, Node2 = _nodeBId }).Wait();
            _sut.Poller.Tick();
        }

        [Given(@"Существует трасса c оборудованием А1 в середине и B1 в конце")]
        public void GivenСуществуетТрассаCОборудованиемАвСерединеИbвКонце()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, @"some title", "", Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _nodeBId, NodeWithRtuId = _rtuNodeId });
            _sut.Poller.Tick();
            _trace = _sut.ReadModel.Traces.Last();
        }

        [Given(@"Для этой трассы задана базовая")]
        public void GivenДляЭтойТрассыЗаданаБазовая()
        {
            var vm = new BaseRefsAssignViewModel(_trace, _sut.ReadModel, _sut.ShellVm.Bus);
            vm.PreciseBaseFilename = SystemUnderTest.Path;
            vm.Save();
            _sut.Poller.Tick();
        }

        [Given(@"Открыта форма для редактирования узла где оборудование А1")]
        public void GivenОткрытаФормаДляРедактированияУзлаГдеОборудованиеА1()
        {
            _vm = new NodeUpdateViewModel(_nodeAId, _sut.ShellVm.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.Bus);
        }
        [Given(@"Открыта форма для редактирования узла где оборудование B1")]
        public void GivenОткрытаФормаДляРедактированияУзлаГдеОборудованиеB1()
        {
            _vm = new NodeUpdateViewModel(_nodeBId, _sut.ShellVm.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.Bus);
        }

        [Then(@"Пункт Удалить доступен для данного оборудования")]
        public void ThenПунктУдалитьДоступенДляДанногоОборудования()
        {
            _vm.EquipmentsInNode.First(e => e.Id == _equipmentA1Id).IsRemoveEnabled.Should().BeTrue();
        }

        [Then(@"Пункт Удалить недоступен для оборудования A1")]
        public void ThenПунктУдалитьНедоступенДляОборудованияA1()
        {
            _vm.EquipmentsInNode.First(e => e.Id == _equipmentA1Id).IsRemoveEnabled.Should().BeFalse();
        }

        [Then(@"Пункт Удалить недоступен для оборудования B1")]
        public void ThenПунктУдалитьНедоступенДляОборудованияB1()
        {
            _vm.EquipmentsInNode.First(e => e.Id == _equipmentB1Id).IsRemoveEnabled.Should().BeFalse();
        }

        [When(@"Пользователь нажимает удалить оборудование")]
        public void WhenПользовательНажимаетУдалитьОборудование()
        {
            var vm = new NodeUpdateViewModel(_nodeAId, _sut.ShellVm.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.Bus);
            vm.RemoveEquipment(new RemoveEquipment() {Id = _equipmentA1Id});
            vm.Cancel();
            _sut.Poller.Tick();
        }

        [Then(@"Оборудование удаляется из трассы")]
        public void ThenОборудованиеУдаляетсяИзТрассы()
        {
            _sut.ReadModel.Traces.Where(t => t.Equipments.Contains(_equipmentA1Id)).Should().BeEmpty();

        }

        [Then(@"Оборудование удаляется")]
        public void ThenОборудованиеУдаляется()
        {
            _sut.ReadModel.Equipments.FirstOrDefault(e=>e.Id == _equipmentA1Id).Should().BeNull();
        }

    }
}
