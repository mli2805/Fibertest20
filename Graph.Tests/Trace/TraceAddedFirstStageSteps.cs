using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAddedFirstStageSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _rtuNodeId;
        private Guid _lastNodeId;

        [Given(@"Существуют РТУ оборудование узлы и отрезки между ними")]
        public void GivenСуществуютРТУУзлыИОтрезкиМеждуНими()
        {
            _sut.CreateFieldForPathFinderTest(out _rtuNodeId, out _lastNodeId);
        }

        [Given(@"И кликнул определить трассу на узле где нет оборудования")]
        public void GivenИКликнулОпределитьТрассуНаУзлеГдеНетОборудования()
        {
            _lastNodeId = _sut.ReadModel.Nodes[5].Id;
            _sut.MapVm.DefineTraceClick(_rtuNodeId, _lastNodeId);
        }

        [Given(@"Между выбираемыми узлами нет пути")]
        public void GivenМеждуВыбираемымиУзламиНетПути()
        {
            _sut.ReadModel.Fibers.RemoveAt(2);
        }

        [Given(@"Но пользователь выбрал узел где есть оборудование и кликнул определить трассу")]
        public void GivenНоПользовательВыбралУзелГдеЕстьОборудованиеИКликнулОпределитьТрассу()
        {
            _sut.MapVm.DefineTraceClick(_rtuNodeId, _lastNodeId);
        }

        [Given(@"Хотя кликнул определить трассу на узле с оборудованием и путь между узлами существует")]
        public void GivenХотяКликнулОпределитьТрассуНаУзлеСОборудованиемИПутьМеждуУзламиСуществует()
        {
            _sut.MapVm.DefineTraceClick(_rtuNodeId, _lastNodeId);
        }

        [Then(@"Сообщение (.*)")]
        public void ThenСообщение(string message)
        {
            _sut.FakeWindowManager.Log
                .OfType<ErrorNotificationViewModel>()
                .Last()
                .ErrorMessage
                .Should().Be(message);
        }

        [Given(@"На вопрос: ""(.*)"" пользователь ответил: ""(.*)""")]
        public void DefineQuestionAnswer(string question, TraceAddedSteps.Answer answer)
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
            {
                var vm = model as QuestionViewModel;
                if (vm == null) return false;
                if (vm.QuestionMessage != question) return false;
                switch (answer)
                {
                    case TraceAddedSteps.Answer.Yes:
                        vm.OkButton();
                        return true;
                    case TraceAddedSteps.Answer.Cancel:
                        vm.CancelButton();
                        return true;
                    default:
                        return false;
                }
            });
        }

        [Then(@"Новая трасса не сохраняется")]
        public void ThenТрассаНеСохраняется()
        {
            _sut.ReadModel.Traces.Count.Should().Be(0);
        }

    }
}
