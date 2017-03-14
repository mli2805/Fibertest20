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
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Equipment _equipment;
        private EquipmentInfoViewModel _equipmentInfoViewModel;

        private const string NewTitleForTest = "New name for old equipment";
        private const EquipmentType NewTypeForTest = EquipmentType.Cross;
        private const int NewLeftCableReserve = 15;
        private const int NewRightCableReserve = 7;
        private const string NewCommentForTest = "New comment for old equipment";

        [Given(@"Существует оборудование")]
        public void GivenСуществуетОборудование()
        {
            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Terminal}).Wait();
            _sut.Poller.Tick();
            _equipment = _sut.ReadModel.Equipments.Single();
        }

        [Given(@"Открыта форма для изменения сущ оборудования")]
        public void GivenОткрытаФормаДляИзмененияСущОборудования()
        {
            _equipmentInfoViewModel = new EquipmentInfoViewModel(_equipment, _sut.ShellVm.Bus);
        }

        [Given(@"Пользователь производит изменения")]
        public void GivenПользовательПроизводитИзменения()
        {
            _equipmentInfoViewModel.Title = NewTitleForTest;
            _equipmentInfoViewModel.Type = NewTypeForTest;
            _equipmentInfoViewModel.CableReserveLeft = NewLeftCableReserve;
            _equipmentInfoViewModel.CableReserveRight = NewRightCableReserve;
            _equipmentInfoViewModel.Comment = NewCommentForTest;
        }

        [When(@"Жмет сохранить")]
        public void WhenЖметСохранить()
        {
            _equipmentInfoViewModel.Save();
            _sut.Poller.Tick();
        }

        [When(@"Жмет Отмена")]
        public void WhenЖметОтмена()
        {
            _equipmentInfoViewModel.Cancel();
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
            _equipmentInfoViewModel.Command.Should().BeNull();
        }

    }
}
