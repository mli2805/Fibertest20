using System;
using System.Collections.Generic;
using FluentAssertions;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAddedThirdStageSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _rtuNodeId;
        private Guid _lastNodeId;

        private const string TraceTitle = "Some trace";
        private const string TraceComment = "Comment for trace";

        private TraceAddViewModel _traceAddViewModel;


        [Given(@"Сформирован набор данных трассы и открыто окно создания трассы")]
        public void GivenСформированНаборДанныхТрассыИОткрытоОкноСозданияТрассы()
        {
            _sut.CreateFieldForPathFinderTest(out _rtuNodeId, out _lastNodeId);

            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer("Accept the path?", Answer.Yes, model));

            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(EquipmentChoiceAnswer.Use, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(EquipmentChoiceAnswer.Use, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(EquipmentChoiceAnswer.Use, model));
        }

        [When(@"Пользователь жмет Применить при пустом имени трассы")]
        public void GivenПользовательЖметПрименитьПриПустомИмениТрассы()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, "", TraceComment, Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new AskAddTrace() { LastNodeId = _lastNodeId, NodeWithRtuId = _rtuNodeId }).Wait();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь вводит название трассы и жмет Сохранить")]
        public void WhenПользовательВводитНазваниеТрассыИЖметСохранить()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, TraceTitle, TraceComment, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new AskAddTrace() { LastNodeId = _lastNodeId, NodeWithRtuId = _rtuNodeId }).Wait();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь жмет Отмена")]
        public void WhenПользовательЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, TraceTitle, TraceComment, Answer.Cancel));
            _sut.ShellVm.ComplyWithRequest(new AskAddTrace() { LastNodeId = _lastNodeId, NodeWithRtuId = _rtuNodeId }).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Окно не закрывается")]
        public void ThenОкноНеЗакрывается()
        {
            _traceAddViewModel.IsClosed = false;
        }

        [Then(@"Окно закрывается")]
        public void ThenОкноЗакрывается()
        {
            _traceAddViewModel.IsClosed = true;
        }

        [Then(@"Новая трасса сохраняется и окно закрывается")]
        public void ThenНоваяТрассаСохраняетсяИОкноЗакрывается()
        {
            _sut.ReadModel.Traces.Count.Should().Be(1);
            _traceAddViewModel.IsClosed = true;
        }

        [Then(@"Трасса не сохраняется")]
        public void ThenТрассаНеСохраняется()
        {
            _sut.ReadModel.Traces.Count.Should().Be(0);
        }


    }
}
