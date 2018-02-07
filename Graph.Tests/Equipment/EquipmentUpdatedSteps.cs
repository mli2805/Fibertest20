using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentUpdatedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _nodeAId, _equipmentA1Id;
        private Guid _nodeBId, _equipmentB1Id;
        private Iit.Fibertest.Graph.Trace _trace;
        private NodeUpdateViewModel _nodeUpdateViewModel;
        private Iit.Fibertest.Graph.Equipment _equipment;
        private int _cutOff;

        [Given(@"Задана трасса c оборудованием А1 в середине и B1 в конце")]
        public void GivenЗаданаТрассаCОборудованиемАвСерединеИbвКонце()
        {
            _trace = _sut.SetTraceFromRtuThrouhgAtoB(out _nodeAId, out _equipmentA1Id, out _nodeBId, out _equipmentB1Id);
            _equipment = _sut.ReadModel.Equipments.First(e => e.Id == _equipmentB1Id);
        }

        [Given(@"Открыта форма изменения узла где лежит А1")]
        public void GivenОткрытаФормаИзмененияУзлаГдеЛежитА()
        {
            _nodeUpdateViewModel = _sut.Container.Resolve<NodeUpdateViewModel>();
            _nodeUpdateViewModel.Initialize(_nodeAId);
        }

        [Given(@"Открыта форма изменения узла где лежит B1")]
        public void GivenОткрытаФормаИзмененияУзлаГдеЛежитB()
        {
            _nodeUpdateViewModel = _sut.Container.Resolve<NodeUpdateViewModel>();
            _nodeUpdateViewModel.Initialize(_nodeBId);
        }

        [Given(@"Задаем базовую")]
        public void GivenЗадаемБазовую()
        {
            var traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_trace.Id);
            var rtuId = traceLeaf.Parent.Id;
            _sut.InitializeRtu(rtuId);

            _sut.AssignBaseRef(traceLeaf, SystemUnderTest.Base1625Lm3, SystemUnderTest.Base1625Lm3, null, Answer.Yes);
            traceLeaf.BaseRefsSet.PreciseId.Should().NotBe(Guid.Empty);
        }

        [Then(@"Для А1 доступно удаление")]
        public void ThenДляАДоступноУдаление()
        {
            _nodeUpdateViewModel.EquipmentsInNode.First(i => i.Id == _equipmentA1Id).IsRemoveEnabled.Should().BeTrue();
        }

        [Then(@"Для А1 НЕ доступно удаление")]
        public void ThenДляА1НеДоступноУдаление()
        {
            _nodeUpdateViewModel.EquipmentsInNode.First(i => i.Id == _equipmentA1Id).IsRemoveEnabled.Should().BeFalse();
        }

        [Then(@"Для B1 НЕ доступно удаление")]
        public void ThenДляB1НеДоступноУдаление()
        {
            _nodeUpdateViewModel.EquipmentsInNode.First(i => i.Id == _equipmentB1Id).IsRemoveEnabled.Should().BeFalse();
        }

        [Given(@"Пользователь кликает изменить B1 вводит новые значения и жмет Сохранить")]
        public void GivenПользовательКликаетИзменитьBВводитНовыеЗначенияИЖметСохранить()
        {
            _sut.FakeWindowManager.RegisterHandler(model=> _sut.EquipmentInfoViewModelHandler(model, Answer.Yes));

            var item = _nodeUpdateViewModel.EquipmentsInNode.First(i => i.Id == _equipmentB1Id);
            item.Command = new UpdateEquipment() {Id = _equipmentB1Id};
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Пользователь кликает изменить B1 вводит новые значения и жмет Отмена")]
        public void GivenПользовательКликаетИзменитьBВводитНовыеЗначенияИЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentInfoViewModelHandler(model, Answer.Cancel));

            var item = _nodeUpdateViewModel.EquipmentsInNode.First(i => i.Id == _equipmentB1Id);
            item.Command = new UpdateEquipment() { Id = _equipmentB1Id };
            _cutOff = _sut.Poller.CurrentEventNumber;
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Все должно быть сохранено")]
        public void ThenВсеДолжноБытьСохранено()
        {
            _equipment.Title.Should().Be(SystemUnderTest.NewTitleForTest);
            _equipment.Type.Should().Be(SystemUnderTest.NewTypeForTest);
            _equipment.CableReserveLeft.Should().Be(SystemUnderTest.NewLeftCableReserve);
            _equipment.CableReserveRight.Should().Be(SystemUnderTest.NewRightCableReserve);
            _equipment.Comment.Should().Be(SystemUnderTest.NewCommentForTest);
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.Id == Guid.Empty).Should().BeNull();
        }

        [Then(@"Комманда не подается")]
        public void ThenКоммандаНеПодается()
        {
            _sut.Poller.CurrentEventNumber.Should().Be(_cutOff);

            _equipment.Title.Should().NotBe(SystemUnderTest.NewTitleForTest);
            _equipment.Type.Should().NotBe(SystemUnderTest.NewTypeForTest);
            _equipment.CableReserveLeft.Should().NotBe(SystemUnderTest.NewLeftCableReserve);
            _equipment.CableReserveRight.Should().NotBe(SystemUnderTest.NewRightCableReserve);
            _equipment.Comment.Should().NotBe(SystemUnderTest.NewCommentForTest);
        }
    }
}
