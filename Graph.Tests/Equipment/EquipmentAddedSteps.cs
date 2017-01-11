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
        private AddEquipmentViewModel _updateEquipmentViewModel;
        private int _cutOff;

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
            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"Открыто окно для добавления оборудования в этот узел")]
        public void GivenAnAddEquipmentWindowOpenedForSaidNode()
        {
            _updateEquipmentViewModel = new AddEquipmentViewModel(_saidNodeId, Guid.Empty, _sut.ReadModel, _sut.Aggregate);
        }

        [When(@"Нажата клавиша Сохранить в окне добавления оборудования")]
        public void WhenSaveButtonOnAddEquipmentWindowPressed()
        {
            _updateEquipmentViewModel.Save();
            _sut.Poller.Tick();
        }

        [When(@"Нажата клавиша Отменить в окне добавления оборудования")]
        public void WhenCancelButtonOnAddEquipmentWindowPressed()
        {
            _updateEquipmentViewModel.Cancel();
        }

        [Then(@"Новое оборудование сохраняется")]
        public void ThenTheNewPieceOfEquipmentGetsSaved()
        {
            _sut.ReadModel.FindEquipmentsByNode(_saidNodeId).Count().Should().Be(1);
        }

        [Then(@"Окно добавления оборудования закрывается")]
        public void ThenTheAddEquipmentWindowGetsClosed()
        {
            _updateEquipmentViewModel.IsClosed.Should().BeTrue();
        }

    }
}