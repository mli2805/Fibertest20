using System;
using System.Linq;
using Caliburn.Micro;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfClient.ViewModels;
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
            _sut.Map.AddNode();
            _sut.Map.AddNode();
            _sut.Poller.Tick();
            _leftNodeId = _sut.ReadModel.Nodes.First().Id;
            _rightNodeId = _sut.ReadModel.Nodes.Last().Id;
            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"Отрезок между левым и правым узлом уже добавлен")]
        public void AddFiber()
        {
            _sut.Map.AddFiber(_leftNodeId, _rightNodeId);
            _sut.Poller.Tick();
            _cutOff = _sut.CurrentEventNumber;
        }

        [When(@"Пользователь кликает добавить отрезок")]
        public void WhenUserClickedAddFiber()
        {
            _sut.Map.AddFiber(_leftNodeId, _rightNodeId);
            _sut.Poller.Tick();
        }

        [Then(@"Новый отрезок сохранен")]
        public void ThenNewEventPersisted()
        {
            _sut.ReadModel.FindFiberByNodes(_leftNodeId, _rightNodeId).Should().NotBe(Guid.Empty);
        }

        [Then(@"Новый отрезок не создается")]
        public void ThenНовыйОтрезокНеСоздается()
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff);
        }
    }
}
