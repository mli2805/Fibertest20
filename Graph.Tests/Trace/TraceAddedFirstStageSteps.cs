using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAddedFirstStageSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _rtuNodeId;
        private Guid _lastNodeId;

        [Given(@"Существуют РТУ оборудование узлы и отрезки между ними")]
        public void GivenСуществуютРтуУзлыИОтрезкиМеждуНими()
        {
            _sut.CreateFieldForPathFinderTest(out _rtuNodeId, out _lastNodeId);
        }

        [Given(@"И кликнул определить трассу на узле где нет оборудования")]
        public void GivenИКликнулОпределитьТрассуНаУзлеГдеНетОборудования()
        {
            _lastNodeId = _sut.ReadModel.Nodes[5].Id;
            _sut.ShellVm.ComplyWithRequest(new AskAddTrace() {LastNodeId = _lastNodeId, NodeWithRtuId = _rtuNodeId})
                .Wait();
        }

        [Given(@"Между выбираемыми узлами нет пути")]
        public void GivenМеждуВыбираемымиУзламиНетПути()
        {
            _sut.ReadModel.Fibers.RemoveAt(2);
        }

        [Given(@"Но пользователь выбрал узел где есть оборудование и кликнул определить трассу")]
        public void GivenНоПользовательВыбралУзелГдеЕстьОборудованиеИКликнулОпределитьТрассу()
        {
            _sut.ShellVm.ComplyWithRequest(new AskAddTrace() { LastNodeId = _lastNodeId, NodeWithRtuId = _rtuNodeId })
                .Wait();
        }

        [Given(@"Хотя кликнул определить трассу на узле с оборудованием и путь между узлами существует")]
        public void GivenХотяКликнулОпределитьТрассуНаУзлеСОборудованиемИПутьМеждуУзламиСуществует()
        {
            _sut.ShellVm.ComplyWithRequest(new AskAddTrace() { LastNodeId = _lastNodeId, NodeWithRtuId = _rtuNodeId })
                .Wait();
        }

        [Then(@"Сообщение (.*)")]
        public void ThenСообщение(string message)
        {
            _sut.FakeWindowManager.Log
                .OfType<NotificationViewModel>()
                .Last()
                .Message
                .Should().Be(message);
        }

        [Given(@"На вопрос: ""(.*)"" пользователь ответил: ""(.*)""")]
        public void DefineQuestionAnswer(string question, Answer answer)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.QuestionAnswer(question, answer, model));
        }

        [Then(@"Новая трасса не сохраняется")]
        public void ThenТрассаНеСохраняется()
        {
            _sut.ReadModel.Traces.Count.Should().Be(0);
        }

    }

}
