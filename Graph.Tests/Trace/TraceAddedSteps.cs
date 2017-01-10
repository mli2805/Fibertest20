using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private readonly MapViewModel _mapViewModel;
        private AddTraceViewModel _addTraceViewModel;
        private ErrorNotificationViewModel _errorNotificationViewModel;
        private Guid _rtuNodeId;
        private Guid _lastNodeId;
        private List<Guid> _traceNodes;


        private int _cutOff;

        public TraceAddedSteps()
        {
            _mapViewModel = new MapViewModel(_sut.Aggregate, _sut.ReadModel);
        }

        [Given(@"Существует два узла")]
        public void GivenСуществуетДваУзла()
        {
            _mapViewModel.AddRtuAtGpsLocation();
            _mapViewModel.AddNode();
            _sut.Poller.Tick();
            _rtuNodeId = _sut.ReadModel.Nodes.First().Id;
            _lastNodeId = _sut.ReadModel.Nodes.Last().Id;
        }

        [Given(@"Между этими узлами есть путь")]
        public void GivenМеждуЭтимиУзламиЕстьПуть()
        {
            _mapViewModel.AddFiber(_rtuNodeId, _lastNodeId);
            _sut.Poller.Tick();
        }

        [Given(@"Пользователь выбрал два узла и кликнул определить трассу")]
        public void GivenПользовательВыбралДваУзлаИКликнулОпределитьТрассу()
        {
            var path = new PathFinder(_sut.ReadModel).FindPath(_rtuNodeId, _lastNodeId);
            if (path != null)
            {
                _traceNodes = path.ToList();
                var equipments = new List<Guid>();
                _addTraceViewModel = new AddTraceViewModel(_sut.ReadModel, _sut.Aggregate, _traceNodes, equipments);
            }
            else
            {
                _errorNotificationViewModel = new ErrorNotificationViewModel("Path couldn't be found!");
            }
        }

        [Then(@"Сообщение о отсутствии пути")]
        public void ThenСообщениеООтсутствииПути()
        {
            _errorNotificationViewModel.IsClosed.Should().BeFalse();
        }


        [Then(@"Открывается окно добавления трассы")]
        public void ThenОткрываетсяОкноДобавленияТрассы()
        {
            _addTraceViewModel.IsClosed.Should().BeFalse();
        }

        [When(@"Пользователь вводит название трассы и жмет Сохранить")]
        public void WhenПользовательВводитНазваниеТрассыИЖметСохранить()
        {
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

        [Then(@"Окно закрывается и трасса не сохраняется")]
        public void ThenОкноЗакрываетсяИТрассаНеСохраняется()
        {
            _sut.ReadModel.Traces.Count.Should().Be(0);
            _addTraceViewModel.IsClosed = true;
        }



    }
}
