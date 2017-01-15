using System;
using System.Linq;
using Caliburn.Micro;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentUpdatedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _equipmentId;
        private EquipmentViewModel _equipmentViewModel;

        private const string NewTitleForTest = "New name for old equipment";
        private const EquipmentType NewTypeForTest = EquipmentType.Cross;
        private const int NewLeftCableReserve = 15;
        private const int NewRightCableReserve = 7;
        private const string NewCommentForTest = "New comment for old equipment";

        [Given(@"Существует оборудование")]
        public void GivenСуществуетОборудование()
        {
            _sut.Map.AddEquipmentAtGpsLocation(EquipmentType.Terminal);
            _sut.Poller.Tick();
            _equipmentId = _sut.ReadModel.Equipments.Single().Id;
        }

        [Given(@"Открыта форма для изменения сущ оборудования")]
        public void GivenОткрытаФормаДляИзмененияСущОборудования()
        {
            _equipmentViewModel = new EquipmentViewModel(Guid.Empty, _equipmentId, null, _sut.ReadModel, _sut.Aggregate);
        }

        [Given(@"Пользователь производит изменения")]
        public void GivenПользовательПроизводитИзменения()
        {
            _equipmentViewModel.Title = NewTitleForTest;
            _equipmentViewModel.Type = NewTypeForTest;
            _equipmentViewModel.CableReserveLeft = NewLeftCableReserve;
            _equipmentViewModel.CableReserveRight = NewRightCableReserve;
            _equipmentViewModel.Comment = NewCommentForTest;
        }

        [When(@"Жмет сохранить")]
        public void WhenЖметСохранить()
        {
            _equipmentViewModel.Save();
            _sut.Poller.Tick();
        }

        [When(@"Жмет Отмена")]
        public void WhenЖметОтмена()
        {
            _equipmentViewModel.Cancel();
            _sut.Poller.Tick();
        }

        [Then(@"Все должно быть сохранено")]
        public void ThenВсеДолжноБытьСохранено()
        {
            var equipment = _sut.ReadModel.Equipments.Single(e => e.Id == _equipmentId);
            equipment.Title.Should().Be(NewTitleForTest);
            equipment.Type.Should().Be(NewTypeForTest);
            equipment.CableReserveLeft.Should().Be(NewLeftCableReserve);
            equipment.CableReserveRight.Should().Be(NewRightCableReserve);
            equipment.Comment.Should().Be(NewCommentForTest);
        }
    }
}
