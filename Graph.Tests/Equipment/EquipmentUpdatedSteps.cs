using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentUpdatedSteps
    {
        private readonly SutForEquipmentOperations _sut = new SutForEquipmentOperations();
        private Guid _nodeAId, _equipmentA1Id;
        private Guid _nodeBId, _equipmentB1Id;
        private Iit.Fibertest.Graph.Trace _trace;
        private NodeUpdateViewModel _nodeUpdateViewModel;
        private Iit.Fibertest.Graph.Equipment _equipment;
        private int _cutOff;

        private const string NewTitleForTest = "New name for old equipment";
        private const EquipmentType NewTypeForTest = EquipmentType.Cross;
        private const int NewLeftCableReserve = 15;
        private const int NewRightCableReserve = 7;
        private const string NewCommentForTest = "New comment for old equipment";


        [Given(@"Задана трасса c оборудованием А1 в середине и B1 в конце")]
        public void GivenЗаданаТрассаCОборудованиемАвСерединеИbвКонце()
        {
            _trace = _sut.SetTraceFromRtuThrouhgAtoB(out _nodeAId, out _equipmentA1Id, out _nodeBId, out _equipmentB1Id);
            _equipment = _sut.ReadModel.Equipments.First(e => e.Id == _equipmentB1Id);
        }

        [Given(@"Открыта форма изменения узла где лежит А1")]
        public void GivenОткрытаФормаИзмененияУзлаГдеЛежитА()
        {
            _nodeUpdateViewModel = new NodeUpdateViewModel(_nodeAId, _sut.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.Bus);
        }

        [Given(@"Открыта форма изменения узла где лежит B1")]
        public void GivenОткрытаФормаИзмененияУзлаГдеЛежитB()
        {
            _nodeUpdateViewModel = new NodeUpdateViewModel(_nodeBId, _sut.ReadModel, _sut.FakeWindowManager, _sut.ShellVm.Bus);
        }

        [Given(@"Задаем базовую")]
        public void GivenЗадаемБазовую()
        {
            var vm = new BaseRefsAssignViewModel(_trace, _sut.ReadModel, _sut.ShellVm.Bus);
            vm.PreciseBaseFilename = SystemUnderTest.Path;
            vm.Save();
            _sut.Poller.Tick();
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
            _sut.Poller.Tick();
        }

        [Given(@"Пользователь кликает изменить B1 вводит новые значения и жмет Отмена")]
        public void GivenПользовательКликаетИзменитьBВводитНовыеЗначенияИЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentInfoViewModelHandler(model, Answer.Cancel));

            var item = _nodeUpdateViewModel.EquipmentsInNode.First(i => i.Id == _equipmentB1Id);
            item.Command = new UpdateEquipment() { Id = _equipmentB1Id };
            _cutOff = _sut.Poller.CurrentEventNumber;
            _sut.Poller.Tick();
        }

        [Then(@"Все должно быть сохранено")]
        public void ThenВсеДолжноБытьСохранено()
        {
            _equipment.Title.Should().Be(NewTitleForTest);
            _equipment.Type.Should().Be(NewTypeForTest);
            _equipment.CableReserveLeft.Should().Be(NewLeftCableReserve);
            _equipment.CableReserveRight.Should().Be(NewRightCableReserve);
            _equipment.Comment.Should().Be(NewCommentForTest);
        }

        [Then(@"Комманда не подается")]
        public void ThenКоммандаНеПодается()
        {
            _sut.Poller.CurrentEventNumber.Should().Be(_cutOff);

            _equipment.Title.Should().NotBe(NewTitleForTest);
            _equipment.Type.Should().NotBe(NewTypeForTest);
            _equipment.CableReserveLeft.Should().NotBe(NewLeftCableReserve);
            _equipment.CableReserveRight.Should().NotBe(NewRightCableReserve);
            _equipment.Comment.Should().NotBe(NewCommentForTest);
        }

    }
}
