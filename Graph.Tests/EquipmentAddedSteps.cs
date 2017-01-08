using System;
using System.Linq;
using FluentAssertions;
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
        private AddEquipmentViewModel _window;
        private int _cutOff;

        public EquipmentAddedSteps()
        {
            _mapViewModel = new MapViewModel(_sut.Aggregate);
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
            _window = new AddEquipmentViewModel(_saidNodeId, _sut.ReadModel, _sut.Aggregate);
        }

        [When(@"Нажата клавиша Сохранить в окне добавления оборудования")]
        public void WhenSaveButtonOnAddEquipmentWindowPressed()
        {
            _sut.AddEquipment();
            _window.Save();
        }

        [When(@"Нажата клавиша Отменить в окне добавления оборудования")]
        public void WhenCancelButtonOnAddEquipmentWindowPressed()
        {
            _window.Cancel();
        }

        [Then(@"Новое оборудование сохраняется")]
        public void ThenTheNewPieceOfEquipmentGetsSaved()
        {
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

        [Then(@"Окно добавления оборудования закрывается")]
        public void ThenTheAddEquipmentWindowGetsClosed()
        {
            _window.IsClosed.Should().BeTrue();
        }

    }
}