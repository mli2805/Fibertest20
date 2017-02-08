using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAddedIntegrationalSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _rtuNodeId;
        private Guid _lastNodeId;

        private const string TraceTitle = "Some trace";
        private const string TraceComment = "Comment for trace";

        [Given(@"Предусловия выполнены")]
        public void GivenПредусловияВыполнены()
        {
            _sut.CreateFieldForPathFinderTest(out _rtuNodeId, out _lastNodeId);

            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer("Accept the path?", Answer.Yes, model));

            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(EquipmentChoiceAnswer.Use, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(EquipmentChoiceAnswer.Use, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(EquipmentChoiceAnswer.Use, model));
        }

        [When(@"Пользователь вводит название и коммент трассы и жмет Сохранить")]
        public void WhenПользовательВводитНазваниеИКомментТрассыИЖметСохранить()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, TraceTitle, TraceComment, Answer.Yes));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() {LastNodeId = _lastNodeId, NodeWithRtuId = _rtuNodeId}).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Трасса сохраняется")]
        public void ThenТрассаСохраняется()
        {
            var trace = _sut.ReadModel.Traces.Single();
            trace.Title.Should().Be(TraceTitle);
            trace.Comment.Should().Be(TraceComment);
        }

        [When(@"Пользователь что-то вводит но жмет Отмена")]
        public void WhenПользовательЧто_ТоВводитНоЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.AddTraceViewHandler(model, TraceTitle, TraceComment, Answer.Cancel));

            _sut.ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = _lastNodeId, NodeWithRtuId = _rtuNodeId }).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Трасса не сохраняется")]
        public void ThenТрассаНеСохраняется()
        {
            _sut.ReadModel.Traces.Count.Should().Be(0);
        }
    }
}
