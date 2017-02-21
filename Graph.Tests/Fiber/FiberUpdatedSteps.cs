﻿using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class FiberUpdatedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _saidFiberId;
        private int _cutOff;

        
        [Given(@"Существует отрезок")]
        public void GivenСуществуетОтрезок()
        {
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.Tick();
            var n1 = _sut.ReadModel.Nodes.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.Tick();
            var n2 = _sut.ReadModel.Nodes.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = n1, Node2 = n2}).Wait();
            _sut.Poller.Tick();
            _saidFiberId = _sut.ReadModel.Fibers.Last().Id;

            _cutOff = _sut.CurrentEventNumber;
        }

        [When(@"Пользователь открыл форму редактирования и нажал сохранить")]
        public void WhenПользовательНажалСохранить()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.FiberUpdateHandler(model, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new RequestUpdateFiber() {Id = _saidFiberId}).Wait();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь открыл форму редактирования и нажал отмена")]
        public void WhenПользовательНажалОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.FiberUpdateHandler(model, Answer.Cancel));
            _sut.ShellVm.ComplyWithRequest(new RequestUpdateFiber() { Id = _saidFiberId }).Wait();
            _sut.Poller.Tick();
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
