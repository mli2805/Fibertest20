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
        private EquipmentInfoViewModel _equipmentInfoViewModel;
        private Iit.Fibertest.Graph.Equipment _equipment; 

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

        [Given(@"Открыта форма изменения узла где лежит B1")]
        public void GivenОткрытаФормаИзмененияУзлаГдеЛежитB()
        {
            _nodeUpdateViewModel = new NodeUpdateViewModel(_nodeBId, _sut.ReadModel, new FakeWindowManager(), _sut.ShellVm.Bus);
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
