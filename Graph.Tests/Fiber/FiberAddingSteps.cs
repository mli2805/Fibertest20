using System;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class FiberAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private readonly MapViewModel _vm;
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private int _cutOff;

        public FiberAddedSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate);
        }

        [Given(@"Левый и правый узлы созданы")]
        public void GivenALeftAndRightNodesCreated()
        {
            _leftNodeId = _sut.AddNode();
            _cutOff = _sut.CurrentEventNumber;
            _rightNodeId = _sut.AddNode();
            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"Отрезок между левым и правым узлом уже добавлен")]
        public void GivenОтрезокМеждуЛевымИПравымУзломУжеДобавлен()
        {
            _vm.AddFiber(_leftNodeId, _rightNodeId);
            _cutOff = _sut.CurrentEventNumber;
        }

        [When(@"Пользователь кликает добавить отрезок")]
        public void WhenUserClickedAddFiber()
        {
            _vm.AddFiber(_leftNodeId, _rightNodeId);
        }

        [Then(@"Новый отрезок сохранен")]
        public void ThenNewEventPersisted()
        {
            _sut.Poller.Tick();
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

        [Then(@"Новый отрезок не создается")]
        public void ThenНовыйОтрезокНеСоздается()
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff);
        }
    }
}
