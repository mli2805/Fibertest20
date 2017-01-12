using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class EquipmentAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _saidNodeId;
        private MapViewModel _mapViewModel;
        private EquipmentViewModel _equipmentViewModel;

        private const string TitleForTest = "Name for equipment";
        private const EquipmentType TypeForTest = EquipmentType.CableReserve;
        private const int LeftCableReserve = 2;
        private const int RightCableReserve = 14;
        private const string CommentForTest = "Comment for equipment";

        public EquipmentAddedSteps()
        {
            _mapViewModel = new MapViewModel(_sut.Aggregate, _sut.ReadModel);
        }

        [Given(@"Добавлен некий узел")]
        public void GivenAContainer_NodeCreated()
        {
            _mapViewModel.AddNode();
            _sut.Poller.Tick();
            _saidNodeId = _sut.ReadModel.Nodes.Single().Id;
        }

        [Given(@"Открыто окно для добавления оборудования в этот узел")]
        public void GivenAnAddEquipmentWindowOpenedForSaidNode()
        {
            _equipmentViewModel = new EquipmentViewModel(_saidNodeId, Guid.Empty, _sut.ReadModel, _sut.Aggregate);
            _equipmentViewModel.Title = TitleForTest;
            _equipmentViewModel.Type = TypeForTest;
            _equipmentViewModel.CableReserveLeft = LeftCableReserve;
            _equipmentViewModel.CableReserveRight = RightCableReserve;
            _equipmentViewModel.Comment = CommentForTest;
        }

        [When(@"Нажата клавиша Сохранить в окне добавления оборудования")]
        public void WhenSaveButtonOnAddEquipmentWindowPressed()
        {
            _equipmentViewModel.Save();
            _sut.Poller.Tick();
        }

        [When(@"Нажата клавиша Отменить в окне добавления оборудования")]
        public void WhenCancelButtonOnAddEquipmentWindowPressed()
        {
            _equipmentViewModel.Cancel();
        }

        [Then(@"Новое оборудование сохраняется")]
        public void ThenTheNewPieceOfEquipmentGetsSaved()
        {
            var equipment = _sut.ReadModel.Equipments.Last();
            equipment.Title.Should().Be(TitleForTest);
            equipment.Type.Should().Be(TypeForTest);
            equipment.CableReserveLeft.Should().Be(LeftCableReserve);
            equipment.CableReserveRight.Should().Be(RightCableReserve);
            equipment.Comment.Should().Be(CommentForTest);
        }

        [Then(@"Окно добавления оборудования закрывается")]
        public void ThenTheAddEquipmentWindowGetsClosed()
        {
            _equipmentViewModel.IsClosed.Should().BeTrue();
        }

    }
}