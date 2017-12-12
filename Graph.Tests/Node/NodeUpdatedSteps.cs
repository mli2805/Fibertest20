using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeUpdatedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _saidNodeId;
        private int _cutOff; // in scenario with no changes and Save button it's interesting that not only node wasn't changed but command wasn't sent
        private NodeUpdateViewModel _nodeUpdateViewModel;

        [Given(@"Ранее был создан узел с именем (.*)")]
        public void CreateNode(string title)
        {
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var nodeId = _sut.ShellVm.GraphReadModel.Nodes.Last().Id;

            _sut.FakeWindowManager.RegisterHandler(model => _sut.NodeUpdateHandler(model, title, @"doesn't matter", Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new UpdateNode() {Id = nodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Добавлен узел")]
        public void CreateNode()
        {
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.EventSourcingTick().Wait();

            _saidNodeId = _sut.ReadModel.Nodes.Last().Id;
        }

        [When(@"Пользователь открыл окно редактирования только что добавленного узла")]
        public void WhenПользовательОткрылОкноРедактированияТолькоЧтоДобавленногоУзла()
        {
            _nodeUpdateViewModel = new NodeUpdateViewModel(_saidNodeId, _sut.ReadModel, new FakeWindowManager(), _sut.ShellVm.C2DWcfManager);
        }

        [Then(@"Кнопка Сохранить запрещена")]
        public void ThenКнопкаСохранитьЗапрещена()
        {
            _nodeUpdateViewModel["Title"].Should().Be(Resources.SID_Title_is_required);
            _nodeUpdateViewModel.IsButtonSaveEnabled.Should().BeFalse();
        }

        [When(@"Пользователь вводит первый символ в поле Название")]
        public void WhenПользовательВводитПервыйСимволВПолеНазвание()
        {
            _nodeUpdateViewModel.Title = @"T";
        }

        [Then(@"Кнопка Сохранить разрешена")]
        public void ThenКнопкаСохранитьРазрешена()
        {
            _nodeUpdateViewModel["Title"].Should().BeNullOrEmpty();
            _nodeUpdateViewModel.IsButtonSaveEnabled.Should().BeTrue();
        }

        [When(@"Пользователь открыл окно редактирования и ничего не изменив нажал Сохранить")]
        public void WhenПользовательОткрылОкноРедактированияИНичегоНеИзменивНажалСохранить()
        {
            _cutOff = _sut.CurrentEventNumber;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.NodeUpdateHandler(model, null, null, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new UpdateNode() { Id = _saidNodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }


        [Given(@"Пользователь ввел название узла (.*)")]
        public void GivenTitleWasSetToBlah_Blah(string title)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.NodeUpdateHandler(model, title, null, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new UpdateNode() { Id = _saidNodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Сохраняется название узла (.*)")]
        public void ThenСохраняетсяНазваниеУзла(string title)
        {
            _sut.ShellVm.GraphReadModel.Nodes.First(n => n.Id == _saidNodeId).Title.Should().Be(title);
        }

        [Given(@"Пользователь ввел комментарий к узлу (.*)")]
        public void GivenПользовательВвелКомментарийКУзлу(string comment)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.NodeUpdateHandler(model, null, comment, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new UpdateNode() { Id = _saidNodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Сохраняется комментарий узла (.*)")]
        public void ThenСохраняетсяКомментарийУзла(string comment)
        {
            _sut.ShellVm.GraphReadModel.Nodes.First(n => n.Id == _saidNodeId).Comment.Should().Be(comment);
        }

        [When(@"Пользователь открыл окно редактирования и что-то изменив нажал Отменить")]
        public void WhenПользовательОткрылОкноРедактированияИЧто_ТоИзменивНажалОтменить()
        {
            _cutOff = _sut.CurrentEventNumber;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.NodeUpdateHandler(model, @"something", @"doesn't matter", Answer.Cancel));
            _sut.ShellVm.ComplyWithRequest(new UpdateNode() { Id = _saidNodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Никаких команд не подается")]
        public void AssertThereAreNoNewEvents()
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff);
        }

    }
}