using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph.Commands;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class FiberRemovedSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private Guid _fiberId;
        

        [Given(@"Есть два узла и отрезок между ними")]
        public void GivenЕстьДваУзлаИОтрезокМеждуНими()
        {
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.Tick();
            _leftNodeId = _sut.ReadModel.Nodes.First().Id;
            _rightNodeId = _sut.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = _leftNodeId, Node2 = _rightNodeId}).Wait();
            _sut.Poller.Tick();
            _fiberId = _sut.ReadModel.Fibers.Single().Id;
        }

        [When(@"Пользователь кликает удалить отрезок")]
        public void WhenПользовательКликаетУдалитьОтрезок()
        {
            _sut.ShellVm.ComplyWithRequest(new RemoveFiber() {Id = _fiberId}).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Отрезок удаляется")]
        public void ThenОтрезокУдаляется()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Id == _fiberId).Should().Be(null);
        }
    }
}
