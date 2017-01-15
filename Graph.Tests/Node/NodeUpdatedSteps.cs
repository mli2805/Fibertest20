using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeUpdatedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _saidNodeId;
        private UpdateNodeViewModel _updateVm;
        private int _cutOff;
        private MapViewModel _mapViewModel;

        public NodeUpdatedSteps()
        {
            _mapViewModel = new MapViewModel(_sut.Aggregate, _sut.ReadModel);
        }

        [Given(@"Ранее был создан узел с именем (.*)")]
        public void CreateNode(string title)
        {
            _mapViewModel.AddNode();
            _sut.Poller.Tick();
            // TODO: Extract into page object
            _updateVm = new UpdateNodeViewModel(
                _sut.ReadModel.Nodes.Last().Id, _sut.ReadModel, _sut.Aggregate);
            _updateVm.Title = title;
            _updateVm.Save();
            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"Добавлен узел")]
        public void CreateNode()
        {
             _mapViewModel.AddNode();
            _sut.Poller.Tick();
            _cutOff = _sut.CurrentEventNumber;

            _saidNodeId = _sut.ReadModel.Nodes.Last().Id;
        }

        [Given(@"Открыто окно для изменения данного узла")]
        public void OpenWindow()
        {
            _updateVm = new UpdateNodeViewModel(_saidNodeId, _sut.ReadModel, _sut.Aggregate);
        }
        [Given(@"Пользователь ввел название узла (.*)")]
        public void GivenTitleWasSetToBlah_Blah(string title)
        {
            _updateVm.Title = title;
        }

        [When(@"Нажата клавиша сохранить")]
        public void Save()
        {
            _updateVm.Save();
            _sut.Poller.Tick();
        }

        [When(@"Нажата клавиша отменить")]
        public void WhenCancelButtonPressed()
        {
            _updateVm.Cancel();
        }

        [Then(@"Никаких команд не подается")]
        public void AssertThereAreNoNewEvents()
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff);
        }
        [Then(@"Измененный узел сохраняется")]
        public void AssertThereAreNewEvents()
        {
            //TODO: replace with an actual check with UI
            _sut.Poller.Tick();
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

        [Then(@"Некая сигнализация ошибки")]
        public void ThenSomeAlert()
        {
            _updateVm["Title"].Should().NotBeNullOrEmpty();
        }
    }
}