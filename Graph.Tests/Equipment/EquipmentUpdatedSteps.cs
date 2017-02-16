using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentUpdatedSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _equipmentId;
        private EquipmentUpdateViewModel _equipmentUpdateViewModel;

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
            _equipmentId = _sut.ReadModel.Equipments.Single().Id;
        }

        [Given(@"Открыта форма для изменения сущ оборудования")]
        public void GivenОткрытаФормаДляИзмененияСущОборудования()
        {
            _equipmentUpdateViewModel = new EquipmentUpdateViewModel(Guid.Empty, _equipmentId, null);
        }

        [Given(@"Пользователь производит изменения")]
        public void GivenПользовательПроизводитИзменения()
        {
            _equipmentUpdateViewModel.Title = NewTitleForTest;
            _equipmentUpdateViewModel.Type = NewTypeForTest;
            _equipmentUpdateViewModel.CableReserveLeft = NewLeftCableReserve;
            _equipmentUpdateViewModel.CableReserveRight = NewRightCableReserve;
            _equipmentUpdateViewModel.Comment = NewCommentForTest;
        }

        [When(@"Жмет сохранить")]
        public void WhenЖметСохранить()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentUpdateHandler(model, Answer.Yes));
        }

        [When(@"Жмет Отмена")]
        public void WhenЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentUpdateHandler(model, Answer.Cancel));
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
