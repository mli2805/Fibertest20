using System;
using System.Linq;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAddedIntegrationalSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
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

            _sut.MapVm.DefineTraceClick(_rtuNodeId, _lastNodeId);
            _sut.Poller.Tick();
        }

        [Then(@"Трасса сохраняется")]
        public void ThenТрассаСохраняется()
        {
            var trace = _sut.ReadModel.Traces.Single();
            trace.Title.Should().Be(TraceTitle);
            trace.Comment.Should().Be(TraceComment);
        }

    }
}
