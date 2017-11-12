using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.StringResources;
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
        private RtuUpdateViewModel _rtuUpdateViewModel;

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

        [Given(@"Пользователь открыл окно нового RTU и ввел название существующего (.*)")]
        public void GivenПользовательОткрылОкноНовогоRtuиВвелНазваниеСуществующего(string title)
        {
            _rtuUpdateViewModel = new RtuUpdateViewModel(_saidRtuId, _sut.ReadModel, _sut.WcfServiceForClient);
            _rtuUpdateViewModel.Title = title;
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

        [Then(@"Кнопка Сохранить заблокирована поле Название подсвечено")]
        public void ThenКнопкаСохранитьЗаблокированаПолеНазваниеПодсвечено()
        {
            _rtuUpdateViewModel.IsButtonSaveEnabled.Should().BeFalse();
            _rtuUpdateViewModel["Title"].Should().Be(Resources.SID_There_is_a_rtu_with_the_same_title);
        }

        [When(@"Пользователь открыл окно редактирования нового RTU")]
        public void WhenПользовательОткрылОкноРедактированияНовогоRtu()
        {
            _rtuUpdateViewModel = new RtuUpdateViewModel(_saidRtuId, _sut.ReadModel, _sut.WcfServiceForClient);
        }

        [Then(@"Кнопка Сохранить пока заблокирована")]
        public void ThenКнопкаСохранитьПокаЗаблокирована()
        {
            _rtuUpdateViewModel["Title"].Should().Be(Resources.SID_Title_is_required);
            _rtuUpdateViewModel.IsButtonSaveEnabled.Should().BeFalse();
        }

        [When(@"Пользователь ввел первый символ в поле Название")]
        public void WhenПользовательВвелПервыйСимволВПолеНазвание()
        {
            _rtuUpdateViewModel.Title = @"R";
        }

        [When(@"Пользователь очищает поле Название")]
        public void WhenПользовательОчищаетПолеНазвание()
        {
            _rtuUpdateViewModel.Title = "";
        }

        [Then(@"Кнопка Сохранить доступна")]
        public void ThenКнопкаСохранитьДоступна()
        {
            _rtuUpdateViewModel["Title"].Should().BeNullOrEmpty();
            _rtuUpdateViewModel.IsButtonSaveEnabled.Should().BeTrue();
        }

    }
}
