using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class FiberRemovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private Guid _fiberId;
        

        [Given(@"Есть два узла и отрезок между ними")]
        public void GivenЕстьДваУзлаИОтрезокМеждуНими()
        {
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.EventSourcingTick();
            _leftNodeId = _sut.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.EventSourcingTick();
            _rightNodeId = _sut.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
            _sut.Poller.EventSourcingTick();
            _fiberId = _sut.ReadModel.Fibers.Last().Id;
        }

        [When(@"Пользователь кликает удалить отрезок")]
        public void WhenПользовательКликаетУдалитьОтрезок()
        {
            _sut.ShellVm.ComplyWithRequest(new RemoveFiber() {Id = _fiberId}).Wait();
            _sut.Poller.EventSourcingTick();
        }

        [Then(@"Отрезок удаляется")]
        public void ThenОтрезокУдаляется()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Id == _fiberId).Should().Be(null);
        }
    }
}
