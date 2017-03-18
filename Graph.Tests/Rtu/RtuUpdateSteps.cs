using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuUpdateSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _firstRtuId;
        private Guid _firstNodeId;
        private Guid _saidRtuId;
        private Guid _saidNodeId;
        private int _cutOff; // in scenario with no changes and Save button it's interesting that not only node wasn't changed but command wasn't sent


        [Given(@"Ранее был создан RTU с именем (.*)")]
        public void CreateRtu(string title)
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation()).Wait();
            _sut.Poller.Tick();
            _firstRtuId = _sut.ShellVm.ReadModel.Rtus.Last().Id;
            _firstNodeId = _sut.ShellVm.ReadModel.Nodes.Last().Id;

            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, title, @"comment doesn't matter", Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new RequestUpdateRtu() { Id = _firstRtuId, NodeId = _firstNodeId }).Wait();
            _sut.Poller.Tick();
        }

        [Given(@"Добавлен RTU")]
        public void CreateRtu()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation()).Wait();
            _sut.Poller.Tick();

            _saidRtuId = _sut.ReadModel.Rtus.Last().Id;
            _saidNodeId = _sut.ReadModel.Nodes.Last().Id;
        }

        [When(@"Пользователь открыл окно редактирования первого RTU и ничего не изменив нажал Сохранить")]
        public void WhenПользовательОткрылОкноРедактированияRtuиНичегоНеИзменивНажалСохранить()
        {
            _cutOff = _sut.CurrentEventNumber;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, null, null, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new RequestUpdateRtu() { Id = _firstRtuId, NodeId = _firstNodeId }).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Команд не подается")]
        public void ThenКомандНеПодается()
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff);
        }

        [Given(@"Пользователь ввел название нового RTU (.*)")]
        public void GivenTitleWasSetToBlah_Blah(string title)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, title, null, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new RequestUpdateRtu() { Id = _saidRtuId, NodeId = _saidNodeId }).Wait();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь открыл окно редактирования RTU и что-то изменив нажал Отменить")]
        public void WhenПользовательОткрылОкноРедактированияRTUИЧто_ТоИзменивНажалОтменить()
        {
            _cutOff = _sut.CurrentEventNumber;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Cancel));
            _sut.ShellVm.ComplyWithRequest(new RequestUpdateRtu() { Id = _saidRtuId, NodeId = _saidNodeId }).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Сохраняется название RTU (.*)")]
        public void ThenСохраняетсяНазваниеУзла(string title)
        {
            _sut.ShellVm.ReadModel.Rtus.First(n => n.Id == _saidRtuId).Title.Should().Be(title);
        }

        [Given(@"Пользователь ввел комментарий к RTU (.*)")]
        public void GivenПользовательВвелКомментарийКУзлу(string comment)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, null, comment, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new RequestUpdateRtu() { Id = _saidRtuId, NodeId = _saidNodeId }).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Сохраняется комментарий RTU (.*)")]
        public void ThenСохраняетсяКомментарийУзла(string comment)
        {
            _sut.ShellVm.ReadModel.Rtus.First(n => n.Id == _saidRtuId).Comment.Should().Be(comment);
        }

        [Then(@"Будет сигнализация ошибки")]
        public void ThenБудетСигнализацияОшибки()
        {
        }

       
    }
}
