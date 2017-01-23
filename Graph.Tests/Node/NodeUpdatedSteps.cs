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
        private NodeUpdateViewModel _nodeUpdateVm;
        private int _cutOff;


        [Given(@"Ранее был создан узел с именем (.*)")]
        public void CreateNode(string title)
        {
            _sut.MapVm.AddNode();
            _sut.Poller.Tick();
            // TODO: Extract into page object
            _nodeUpdateVm = new NodeUpdateViewModel(
                _sut.ReadModel.Nodes.Last().Id, _sut.ReadModel, _sut.Aggregate);
            _nodeUpdateVm.Title = title;
            _nodeUpdateVm.Save();
            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"Добавлен узел")]
        public void CreateNode()
        {
            _sut.MapVm.AddNode();
            _sut.Poller.Tick();
            _cutOff = _sut.CurrentEventNumber;

            _saidNodeId = _sut.ReadModel.Nodes.Last().Id;
        }

        [Given(@"Открыто окно для изменения данного узла")]
        public void OpenWindow()
        {
            _nodeUpdateVm = new NodeUpdateViewModel(_saidNodeId, _sut.ReadModel, _sut.Aggregate);
        }
        [Given(@"Пользователь ввел название узла (.*)")]
        public void GivenTitleWasSetToBlah_Blah(string title)
        {
            _nodeUpdateVm.Title = title;
        }

        [Given(@"Пользователь ввел какой-то комментарий к узлу")]
        public void GivenПользовательВвелКакой_ТоКомментарийКУзлу()
        {
            _nodeUpdateVm.Comment = "Doesn't matter";
        }

        [When(@"Нажата клавиша сохранить")]
        public void Save()
        {
            _nodeUpdateVm.Save();
            _sut.Poller.Tick();
        }

        [When(@"Нажата клавиша отменить")]
        public void WhenCancelButtonPressed()
        {
            _nodeUpdateVm.Cancel();
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
            _nodeUpdateVm["Title"].Should().NotBeNullOrEmpty();
        }
    }
}