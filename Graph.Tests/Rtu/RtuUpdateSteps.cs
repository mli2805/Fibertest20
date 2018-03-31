using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuUpdateSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _saidRtuId;
        private Guid _saidNodeId;
        private int _cutOff; // in scenario with no changes and Save button it's interesting that not only node wasn't changed but command wasn't sent
        private RtuUpdateViewModel _rtuUpdateViewModel;

        [Given(@"Ранее был создан RTU с именем (.*)")]
        public void CreateRtu(string title)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, title, @"doesn't matter", Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation());
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Добавлен RTU с именем (.*)")]
        public void GivenДобавленRtuсИменем(string title)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, title, @"doesn't matter", Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation());
            _sut.Poller.EventSourcingTick().Wait();

            _saidRtuId = _sut.ReadModel.Rtus.Last().Id;
            _saidNodeId = _sut.ReadModel.Nodes.Last().NodeId;
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
            _sut.GraphReadModel.GrmRtuRequests.UpdateRtu(new RequestUpdateRtu() { RtuId = _saidRtuId, NodeId = _saidNodeId });
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Пользователь открыл окно нового RTU и ввел название существующего (.*)")]
        public void GivenПользовательОткрылОкноНовогоRtuиВвелНазваниеСуществующего(string title)
        {
            _rtuUpdateViewModel = _sut.ClientContainer.Resolve<RtuUpdateViewModel>();
            _rtuUpdateViewModel.Initialize(_saidRtuId);
            _rtuUpdateViewModel.Title = title;
        }

        [When(@"Пользователь открыл окно редактирования RTU и что-то изменив нажал Отменить")]
        public void WhenПользовательОткрылОкноРедактированияRTUИЧто_ТоИзменивНажалОтменить()
        {
            _cutOff = _sut.CurrentEventNumber;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Cancel));
            _sut.GraphReadModel.GrmRtuRequests.UpdateRtu(new RequestUpdateRtu() { RtuId = _saidRtuId, NodeId = _saidNodeId });
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Сохраняется название RTU (.*)")]
        public void ThenСохраняетсяНазваниеУзла(string title)
        {
           _sut.ReadModel.Rtus.First(n => n.Id == _saidRtuId).Title.Should().Be(title);
        }

        [Given(@"Пользователь ввел комментарий к RTU (.*)")]
        public void GivenПользовательВвелКомментарийКУзлу(string comment)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, null, comment, Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.UpdateRtu(new RequestUpdateRtu() { RtuId = _saidRtuId, NodeId = _saidNodeId });
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Сохраняется комментарий RTU (.*)")]
        public void ThenСохраняетсяКомментарийУзла(string comment)
        {
           _sut.ReadModel.Rtus.First(n => n.Id == _saidRtuId).Comment.Should().Be(comment);
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
            _rtuUpdateViewModel = _sut.ClientContainer.Resolve<RtuUpdateViewModel>();
            _rtuUpdateViewModel.Initialize(_saidRtuId);
        }

     
        [Then(@"Кнопка Сохранить становится недоступна")]
        public void ThenКнопкаСохранитьСтановитсяНедоступна()
        {
            _rtuUpdateViewModel["Title"].Should().Be(Resources.SID_Title_is_required);
            _rtuUpdateViewModel.IsButtonSaveEnabled.Should().BeFalse();
        }



        [When(@"Пользователь очищает поле Название")]
        public void WhenПользовательОчищаетПолеНазвание()
        {
            _rtuUpdateViewModel.Title = "";
        }

       
    }
}
