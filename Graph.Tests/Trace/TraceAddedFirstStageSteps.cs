using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAddedFirstStageSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _rtuNodeId;
        private Guid _lastNodeId;
        private Guid _wrongNodeId, _wrongNodeWithEqId;

        private int _traceCountCutOff;

        [Given(@"Существуют РТУ оборудование узлы и отрезки между ними")]
        public void GivenСуществуютРтуУзлыИОтрезкиМеждуНими()
        {
            _sut.CreateFieldForPathFinderTest(out _rtuNodeId, out _lastNodeId, out _wrongNodeId, out _wrongNodeWithEqId);
        }

        [Given(@"И кликнул определить трассу на узле где нет оборудования")]
        public void GivenИКликнулОпределитьТрассуНаУзлеГдеНетОборудования()
        {

            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.AddTrace(
                new RequestAddTrace() {LastNodeId = _wrongNodeId, NodeWithRtuId = _rtuNodeId});
        }

        [Given(@"Между выбираемыми узлами нет пути")]
        public void GivenМеждуВыбираемымиУзламиНетПути()
        {
            _sut.ReadModel.Fibers.RemoveAt(2);
        }

        [Given(@"Но пользователь выбрал узел где есть оборудование и кликнул определить трассу")]
        public void GivenНоПользовательВыбралУзелГдеЕстьОборудованиеИКликнулОпределитьТрассу()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.AddTrace(
                new RequestAddTrace() { LastNodeId = _wrongNodeWithEqId, NodeWithRtuId = _rtuNodeId });
        }

        [Given(@"Хотя кликнул определить трассу на узле с оборудованием и путь между узлами существует")]
        public void GivenХотяКликнулОпределитьТрассуНаУзлеСОборудованиемИПутьМеждуУзламиСуществует()
        {
            _sut.GraphReadModel.AddTrace(new RequestAddTrace() { LastNodeId = _lastNodeId, NodeWithRtuId = _rtuNodeId });
        }

        [Then(@"Выскакивает сообщение о необходимости оборудования в последнем узле")]
        public void ThenВыскакиваетСообщениеОНеобходимостиОборудованияВПоследнемУзле()
        {
            _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last()
                .Lines[0].Line
                .Should().Be(Resources.SID_Last_node_of_trace_must_contain_some_equipment);
        }

        [Then(@"Выскакивает сообщение о невозможности проложить путь")]
        public void ThenВыскакиваетСообщениеОНевозможностиПроложитьПуть()
        {
            _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last()
                .Lines[0].Line
                .Should().Be(Resources.SID_Path_couldn_t_be_found);
        }

        [Given(@"На вопрос: ""(.*)"" пользователь ответил: ""(.*)""")]
        public void DefineQuestionAnswer(string question, Answer answer)
        {
            _traceCountCutOff =_sut.ReadModel.Traces.Count;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.OneLineMessageBoxAnswer(Resources.SID_Accept_the_path, answer, model));
        }

        [Then(@"Новая трасса не сохраняется")]
        public void ThenТрассаНеСохраняется()
        {
            _sut.ReadModel.Traces.Count.Should().Be(_traceCountCutOff);
        }

    }

}
