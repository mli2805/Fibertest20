using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _rtuNodeId;
        private Guid _lastNodeId;
        private AddTraceViewModel _addTraceViewModel;

        [Given(@"Существует набор узлов и отрезков")]
        public void GivenСуществуетНаборУзловИОтрезков()
        {
            _sut.CreateFieldForPathFinderTest(out _rtuNodeId, out _lastNodeId);
        }


        [Given(@"Пользователь выбрал узел где есть оборудование")]
        public void GivenПользовательКликнулНаУзлеГдеЕстьОборудование()
        {
            _lastNodeId = _sut.ReadModel.Equipments.Last().NodeId;
        }

        [Given(@"И кликнул определить трассу")]
        public void GivenПользовательВыбралДваУзлаИКликнулОпределитьТрассу()
        {
            _sut.MapVm.DefineTraceClick(_rtuNodeId, _lastNodeId);

        }

        [Then(@"Открывается окно добавления трассы")]
        public void ThenОткрываетсяОкноДобавленияТрассы()
        {
            _addTraceViewModel = _sut.FakeWindowManager.Log.OfType<AddTraceViewModel>().Last();
            _addTraceViewModel.IsClosed.Should().BeFalse();
        }

        [When(@"Пользователь НЕ вводит имя трассы и жмет Сохранить")]
        public void WhenПользовательНеВводитНазваниеТрассыИЖметСохранить()
        {
            _addTraceViewModel.Title = "";
            _addTraceViewModel.Save();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь вводит название трассы и жмет Сохранить")]
        public void WhenПользовательВводитНазваниеТрассыИЖметСохранить()
        {
            _addTraceViewModel.Title = "Doesn't matter";
            _addTraceViewModel.Save();
            _sut.Poller.Tick();
        }

        [Then(@"Новая трасса сохраняется и окно закрывается")]
        public void ThenНоваяТрассаСохраняетсяИОкноЗакрывается()
        {
            _sut.ReadModel.Traces.Count.Should().Be(1);
            _addTraceViewModel.IsClosed = true;
        }

        [When(@"Пользователь жмет Отмена")]
        public void WhenПользовательЖметОтмена()
        {
            _addTraceViewModel.Cancel();
        }

        [Then(@"Окно не закрывается")]
        public void ThenОкноНеЗакрывается()
        {
            _addTraceViewModel.IsClosed = false;
        }

        [Then(@"Окно закрывается")]
        public void ThenОкноЗакрывается()
        {
            _addTraceViewModel.IsClosed = true;
        }

        [Then(@"Трасса не сохраняется")]
        public void ThenТрассаНеСохраняется()
        {
            _sut.ReadModel.Traces.Count.Should().Be(0);
        }

        [Then(@"На вопрос: ""(.*)"" собираются ответить: ""(.*)""")]
        public void DefineQuestionAnswer(string question, Answer answer)
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
            {
                var vm = model as QuestionViewModel;
                if (vm == null) return false;
                if (vm.QuestionMessage != question) return false;
                switch (answer)
                {
                    case Answer.Yes:
                        vm.OkButton();
                        return true;
                    case Answer.Cancel:
                        vm.CancelButton();
                        return true;
                    default:
                        return false;
                }
            });

        }

        public enum Answer
        {
            Yes,
            Cancel
        }
    }
}
