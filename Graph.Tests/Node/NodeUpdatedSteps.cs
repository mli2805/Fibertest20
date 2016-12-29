using System;
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
        private UpdateNodeViewModel _window;
        private int _cutOff;

        [Given(@"Добавлен узел")]
        public void CreateNode()
        {
            _saidNodeId = _sut.AddNode();
            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"Ранее был создан узел с именем (.*)")]
        public void CreateNode(string title)
        {
            _saidNodeId = _sut.AddNode();
            _sut.UpdateNode(_saidNodeId, title);
            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"Открыто окно для изменения данного узла")]
        public void OpenWindow()
        {
            _window = new UpdateNodeViewModel(_saidNodeId, _sut.ReadModel, _sut.Aggregate);
        }
        [Given(@"Пользователь ввел название узла (.*)")]
        public void GivenTitleWasSetToBlah_Blah(string title)
        {
            _window.Title = title;
        }

        [When(@"Нажата клавиша сохранить")]
        public void Save()
        {
            _window.Save();
        }

        [When(@"Нажата клавиша отменить")]
        public void WhenCancelButtonPressed()
        {
            _window.Cancel();
        }

        [Then(@"Никаких команд не подается")]
        public void AssertThereAreNoNewEvents()
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff);
        }
        [Then(@"Окно закрывается")]
        public void AssertTheWindowIsClosed()
        {
            _window.IsClosed.Should().BeTrue();
        }
        [Then(@"Измененный узел сохраняется")]
        public void AssertThereAreNewEvents()
        {
            //TODO: replace with an actual check with UI
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

        [Then(@"Некая сигнализация ошибки")]
        public void ThenSomeAlert()
        {
            _window.Error.Should().NotBeNullOrEmpty();
        }
        [Then(@"Окно не закрывается")]
        public void ThenTheWindowIsNotClosed()
        {
            _window.IsClosed.Should().BeFalse();
        }

    }
}