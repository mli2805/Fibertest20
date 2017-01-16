﻿using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class FiberUpdatedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private UpdateFiberViewModel _window;
        private Guid _saidFiberId;
        private int _cutOff;

        
        [Given(@"Существует отрезок")]
        public void GivenСуществуетОтрезок()
        {
            _sut.MapVm.AddNode();
            _sut.Poller.Tick();
            var n1 = _sut.ReadModel.Nodes.Last().Id;

            _sut.MapVm.AddNode();
            _sut.Poller.Tick();
            var n2 = _sut.ReadModel.Nodes.Last().Id;

            _sut.MapVm.AddFiber(n1,n2);
            _sut.Poller.Tick();
            _saidFiberId = _sut.ReadModel.Fibers.Last().Id;

            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"Пользователь открывает форму редактирования отрезка")]
        public void GivenПользовательОткрываетФормуРедактированияОтрезка()
        {
            _window = new UpdateFiberViewModel(_saidFiberId, _sut.ReadModel, _sut.Aggregate);
        }

        [When(@"Пользователь нажал сохранить")]
        public void WhenПользовательНажалСохранить()
        {
            _window.Save();
        }

        [When(@"Пользователь нажал отмена")]
        public void WhenПользовательНажалОтмена()
        {
            _window.Cancel();
        }

        [Then(@"Отрезок должен измениться")]
        public void ThenОтрезокДолженИзмениться()
        {
            _sut.Poller.Tick();
            _sut.CurrentEventNumber.Should().Be(_cutOff+1);
        }

        [Then(@"Команда не отсылается")]
        public void ThenКомандаНеОтсылается()
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff);
        }
    }
}
