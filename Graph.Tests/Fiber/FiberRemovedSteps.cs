﻿using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests.Fiber
{
    [Binding]
    public sealed class FiberRemovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private readonly MapViewModel _vm;
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private Guid _fiberId;

        public FiberRemovedSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate);
        }

        [Given(@"Есть два узла и отрезок между ними")]
        public void GivenЕстьДваУзлаИОтрезокМеждуНими()
        {
            _vm.AddNode();
            _vm.AddNode();
            _sut.Poller.Tick();
            _leftNodeId = _sut.ReadModel.Nodes.First().Id;
            _rightNodeId = _sut.ReadModel.Nodes.Last().Id;
            _vm.AddFiber(_leftNodeId, _rightNodeId);
            _sut.Poller.Tick();
            _fiberId = _sut.ReadModel.Fibers.Single().Id;
        }

        [When(@"Пользователь кликает удалить отрезок")]
        public void WhenПользовательКликаетУдалитьОтрезок()
        {
            _vm.RemoveFiber(_fiberId);
            _sut.Poller.Tick();
        }

        [Then(@"Отрезок удаляется")]
        public void ThenОтрезокУдаляется()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Id == _fiberId).Should().Be(null);
        }
    }
}
