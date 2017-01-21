using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
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
            _sut.MapVm.AddRtuAtGpsLocation();
            _sut.Poller.Tick();
            _rtuNodeId = _sut.ReadModel.Rtus.Last().NodeId;
            _sut.MapVm.AddEquipmentAtGpsLocation(EquipmentType.Terminal);
            _sut.Poller.Tick();
            _lastNodeId = _sut.ReadModel.Equipments.Last().NodeId;

            _sut.CreateFieldForPathFinderTest(_rtuNodeId, _lastNodeId);
        }

        [Given(@"Пользователь выбрал узел где нет оборудования")]
        public void GivenПользовательКликнулНаУзлеГдеНетОборудования()
        {
            _lastNodeId = _sut.ReadModel.Nodes[5].Id;
        }

        [Given(@"Пользователь выбрал узел где есть оборудование")]
        public void GivenПользовательКликнулНаУзлеГдеЕстьОборудование()
        {
            _lastNodeId = _sut.ReadModel.Equipments.Last().NodeId;
        }

        [Given(@"Между выбираемыми узлами нет пути")]
        public void GivenМеждуВыбираемымиУзламиНетПути()
        {
            _sut.ReadModel.Fibers.RemoveAt(2);
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

        [Then(@"Сообщение (.*)")]
        public void ThenСообщение(string message)
        {
            _sut.FakeWindowManager.Log
                .OfType<ErrorNotificationViewModel>()
                .Last()
                .ErrorMessage
                .Should().Be(message);
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
        [Then(@"На предложение выбрать оборудование собираются ответить: ""(.*)""")]
        public void DefineEquipmentChoiceAnswer(EquipmentChoiceAnswer answer)
        {
            _sut.FakeWindowManager.RegisterHandler(model =>
                {
                    var vm = model as EquipmentChoiceViewModel;
                    if (vm == null) return false;
                    switch (answer)
                    {
                        case EquipmentChoiceAnswer.Use:
                            vm.UseButton();
                            return true;
                        case EquipmentChoiceAnswer.UseAndSetupName:
                            vm.UseAndSetupNameButton();
                            return true;
                        case EquipmentChoiceAnswer.Cancel:
                            vm.CancelButton();
                            return true;
                        default:
                            return false;
                    }
                });

        }

    }

    public enum EquipmentChoiceAnswer
    {
        Use, UseAndSetupName, Cancel
    }
    public enum Answer
    {
        Yes, Cancel
    }
}
