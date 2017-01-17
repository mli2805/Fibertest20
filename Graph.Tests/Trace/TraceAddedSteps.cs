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

        [Given(@"Между выбираемыми узлами нет пути")]
        public void GivenМеждуВыбираемымиУзламиНетПути()
        {
            _sut.ReadModel.Fibers.RemoveAt(2);
        }

        [Given(@"Пользователь выбрал два узла и кликнул определить трассу")]
        public void GivenПользовательВыбралДваУзлаИКликнулОпределитьТрассу()
        {
            _sut.MapVm.DefineTrace(_rtuNodeId, _lastNodeId);
        }

        [Then(@"Открывается окно добавления трассы")]
        public void ThenОткрываетсяОкноДобавленияТрассы()
        {
            _sut.MapVm.AddTraceViewModel.IsClosed.Should().BeFalse();
        }

        [When(@"Пользователь вводит название трассы и жмет Сохранить")]
        public void WhenПользовательВводитНазваниеТрассыИЖметСохранить()
        {
            _sut.MapVm.AddTraceViewModel.Save();
            _sut.Poller.Tick();
        }

        [Then(@"Новая трасса сохраняется и окно закрывается")]
        public void ThenНоваяТрассаСохраняетсяИОкноЗакрывается()
        {
            _sut.ReadModel.Traces.Count.Should().Be(1);
            _sut.MapVm.AddTraceViewModel.IsClosed = true;
        }

        [When(@"Пользователь жмет Отмена")]
        public void WhenПользовательЖметОтмена()
        {
            _sut.MapVm.AddTraceViewModel.Cancel();
        }

        [Then(@"Окно закрывается и трасса не сохраняется")]
        public void ThenОкноЗакрываетсяИТрассаНеСохраняется()
        {
            _sut.ReadModel.Traces.Count.Should().Be(0);
            _sut.MapVm.AddTraceViewModel.IsClosed = true;
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
    }


}
