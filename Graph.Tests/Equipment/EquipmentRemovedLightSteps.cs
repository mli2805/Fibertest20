using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentRemovedLightSteps
    {
        private readonly SutForEquipment _sut = new SutForEquipment();
        private Guid _nodeAId, _equipmentA1Id;
        private Guid _notInTraceEquipmentId;
        private NodeUpdateViewModel _vm;
        private Iit.Fibertest.Graph.Trace _trace;

        [Given(@"Существует узел с оборудованием")]
        public void GivenСуществуетУзелСОборудованием()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _nodeAId = _sut.ReadModel.Nodes.Last().Id;
            _equipmentA1Id = _sut.ReadModel.Equipments.Last().Id;
        }

        [Given(@"Существует трасса не проходящая через этот узел")]
        public void GivenСуществуетТрассаНеПроходящаяЧерезЭтотУзел()
        {
            _sut.CreateTraceRtuEmptyTerminal();
        }

        [Given(@"Есть трасса и в последнем узле трассы есть оборудование неиспользуемое трассой")]
        public void GivenЕстьТрассаИвПоследнемУзлеТрассыЕстьОборудованиеНеиспользуемоеТрассой()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            var nodeId = _trace.Nodes.Last();

            _sut.FakeWindowManager.RegisterHandler(model =>
                _sut.TraceChoiceHandler(model, new List<Guid>(), Answer.Yes));
            _sut.FakeWindowManager.RegisterHandler(model =>
                _sut.EquipmentInfoViewModelHandler(model, Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentIntoNode() {NodeId = nodeId}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _notInTraceEquipmentId = _sut.ReadModel.Equipments.Last().Id;
        }


        [When(@"Открыта форма редактирования этого узла")]
        public void WhenОткрытаФормаРедактированияЭтогоУзла()
        {
            _vm = new NodeUpdateViewModel(_nodeAId, _sut.ReadModel, new FakeWindowManager(), _sut.ShellVm.C2DWcfManager);
            _vm.EquipmentsInNode.Count.Should().Be(1);
            _vm.EquipmentsInNode.First().IsRemoveEnabled.Should().BeTrue();
        }

        [When(@"Открыта форма редактирования последнего узла трассы")]
        public void WhenОткрытаФормаРедактированияПоследнегоУзлаТрассы()
        {
            _vm = new NodeUpdateViewModel(_trace.Nodes.Last(), _sut.ReadModel, new FakeWindowManager(), _sut.ShellVm.C2DWcfManager);
            _vm.EquipmentsInNode.Count.Should().Be(2);
            _vm.EquipmentsInNode.First(item => item.Id == _notInTraceEquipmentId).IsRemoveEnabled.Should().BeTrue();
        }

        [Then(@"Удаление оборудования используемого трассой в последнем узле должно быть запрещено")]
        public void ThenУдалениеОборудованияИспользуемогоТрассойВПоследнемУзлеДолжноБытьЗапрещено()
        {
            _vm.EquipmentsInNode.First(item => item.Id == _notInTraceEquipmentId).IsRemoveEnabled.Should().BeTrue();
            _vm.EquipmentsInNode.First(item => item.Id != _notInTraceEquipmentId).IsRemoveEnabled.Should().BeFalse();
        }

        [When(@"Пользователь жмет удалить оборудование")]
        public void WhenПользовательЖметУдалитьОборудование()
        {
            _vm.RemoveEquipment(new RemoveEquipment() { Id = _equipmentA1Id });
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь жмет удалить неиспользуемое оборудование")]
        public void WhenПользовательЖметУдалитьНеиспользуемоеОборудование()
        {
            _vm.RemoveEquipment(new RemoveEquipment() { Id = _vm.EquipmentsInNode.First(item => item.Id == _notInTraceEquipmentId).Id });
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"И оборудование удаляется")]
        public void ThenИОборудованиеУдаляется()
        {
            _sut.ReadModel.Equipments.Count(e => e.NodeId == _nodeAId).ShouldBeEquivalentTo(1);
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == _equipmentA1Id).Should().BeNull();
        }

        [Then(@"Неиспользуемое оборудование удаляется")]
        public void ThenНеиспользуемоеОборудованиеУдаляется()
        {
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == _notInTraceEquipmentId).Should().BeNull();
        }

    }
}