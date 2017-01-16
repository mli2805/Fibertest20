﻿using System;
using System.Collections.Generic;
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
        private AddTraceViewModel _addTraceViewModel;
        private ErrorNotificationViewModel _errorNotificationViewModel;
        private Guid _rtuNodeId;
        private Guid _lastNodeId;
        private List<Guid> _traceNodes;
        private List<Guid> _traceEquipments;


        [Given(@"Существует два узла")]
        public void GivenСуществуетДваУзла()
        {
            _sut.MapVm.AddRtuAtGpsLocation();
            _sut.MapVm.AddEquipmentAtGpsLocation(EquipmentType.Cross);
            _sut.Poller.Tick();
            _rtuNodeId = _sut.ReadModel.Nodes.First().Id;
            _traceEquipments = new List<Guid>() {_sut.ReadModel.Rtus.Single().Id, _sut.ReadModel.Equipments.Single().Id};
            _lastNodeId = _sut.ReadModel.Nodes.Last().Id;
        }

        [Given(@"Между этими узлами есть путь")]
        public void GivenМеждуЭтимиУзламиЕстьПуть()
        {
            _sut.MapVm.AddFiber(_rtuNodeId, _lastNodeId);
            _sut.Poller.Tick();
        }

        [Given(@"Пользователь выбрал два узла и кликнул определить трассу")]
        public void GivenПользовательВыбралДваУзлаИКликнулОпределитьТрассу()
        {
            var path = new PathFinder(_sut.ReadModel).FindPath(_rtuNodeId, _lastNodeId);
            if (path != null)
            {
                _traceNodes = path.ToList();
                _addTraceViewModel = new AddTraceViewModel(_sut.ReadModel, _sut.Aggregate, _traceNodes, _traceEquipments);
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

        [Then(@"\?\?\?(.*)")]
        public void Then(string expected)
        {
            _sut.FakeWindowManager.Log
                .OfType<ErrorNotificationViewModel>()
                .Last()
                .ErrorMessage
                .Should().Be(expected);
        }

    }

    
}
