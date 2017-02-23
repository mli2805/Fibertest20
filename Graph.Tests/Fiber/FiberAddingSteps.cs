﻿using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class FiberAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private int _cutOff;


        [Given(@"Левый и правый узлы созданы")]
        public void GivenALeftAndRightNodesCreated()
        {
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.Tick();
            _leftNodeId = _sut.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.Tick();
            _rightNodeId = _sut.ReadModel.Nodes.Last().Id;
            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"Отрезок между левым и правым узлом уже добавлен")]
        public void AddFiber()
        {
            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
            _sut.Poller.Tick();
            _cutOff = _sut.CurrentEventNumber;
        }

        [When(@"Пользователь кликает добавить отрезок")]
        public void WhenUserClickedAddFiber()
        {
            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Новый отрезок сохранен")]
        public void ThenNewEventPersisted()
        {
            _sut.ReadModel.Fibers.Where(f => f.Node1 == _leftNodeId && f.Node2 == _rightNodeId ||
                                             f.Node2 == _leftNodeId && f.Node1 == _rightNodeId).Should().NotBeNull();
        }

        [Then(@"Появится сообщение что есть такое волокно")]
        public void ThenПоявитсяСообщениеЧтоЕстьТакоеВолокно()
        {
            _sut.FakeWindowManager.Log
                .OfType<NotificationViewModel>()
                .Last()
                .Message
                .Should().Be(Resources.SID_There_s_such_a_fiber_already);
        }

        [Then(@"Новый отрезок не создается")]
        public void ThenНовыйОтрезокНеСоздается()
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff);
        }
    }
}
