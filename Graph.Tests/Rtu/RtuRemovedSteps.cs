using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuRemovedSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private Guid _rtuNodeId; // нужно сохранить отдельно, т.к. после удаления РТУ негде взять

        private Iit.Fibertest.Graph.Node _endTraceNode;


        [Given(@"Существует РТУ")]
        public void GivenСуществуетРту()
        {
            _sut.ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation()).Wait();
            _sut.Poller.Tick();
            _rtu = _sut.ReadModel.Rtus.Single();
            _rtuNodeId = _rtu.NodeId;
        }

        [Given(@"Существует несколько отрезков от РТУ")]
        public void GivenСуществуетНесколькоОтрезковОтРту()
        {
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.Tick();
            var n1 = _sut.ReadModel.Nodes.Last();
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.Tick();
            var n2 = _sut.ReadModel.Nodes.Last();
            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type= EquipmentType.Terminal}).Wait();
            _sut.Poller.Tick();
            _endTraceNode = _sut.ReadModel.Nodes.Last();
            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = _rtu.NodeId , Node2 = n1.Id }).Wait();
            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = n1.Id, Node2 = _endTraceNode.Id }).Wait();
            _sut.ShellVm.ComplyWithRequest(new AddFiber() {Node1 = _rtu.NodeId , Node2 = n2.Id }).Wait();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь кликает на РТУ удалить")]
        public void WhenПользовательКликаетНаРтуУдалить()
        {
            _sut.ShellVm.ComplyWithRequest(new RemoveRtu() {Id = _rtu.Id}).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"РТУ удаляется")]
        public void ThenРтуУдаляется()
        {
            _sut.ReadModel.Rtus.Count.Should().Be(0);
        }

        [Then(@"Узел под РТУ и присоединенные к нему отрезки удаляются")]
        public void ThenУзелПодРтуиПрисоединенныеКНемуОтрезкиУдаляются()
        {
            _sut.ReadModel.Nodes.FirstOrDefault(n => n.Id == _rtuNodeId).Should().Be(null);
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Node1 == _rtuNodeId || f.Node2 == _rtuNodeId).Should().BeNull();
        }
    }
}