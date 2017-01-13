using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuRemovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private readonly MapViewModel _vm;
        private Iit.Fibertest.Graph.Rtu _rtu;
        private Guid _rtuNodeId; // нужно сохранить отдельно, т.к. после удаления РТУ негде взять
        private List<Guid> _traceEquipment = new List<Guid>();

        private Iit.Fibertest.Graph.Node _endTraceNode;


        public RtuRemovedSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate, _sut.ReadModel);
        }

        [Given(@"Существует РТУ")]
        public void GivenСуществуетРТУ()
        {
            _vm.AddRtuAtGpsLocation();
            _sut.Poller.Tick();
            _rtu = _sut.ReadModel.Rtus.Single();
            _rtuNodeId = _rtu.NodeId;
            _traceEquipment = new List<Guid>() {_rtu.Id};
        }

        [Given(@"Существует несколько отрезков от РТУ")]
        public void GivenСуществуетНесколькоОтрезковОтРТУ()
        {
            _vm.AddNode();
            _sut.Poller.Tick();
            var n1 = _sut.ReadModel.Nodes.Last();
            _traceEquipment.Add(Guid.Empty);
            _vm.AddNode();
            _sut.Poller.Tick();
            var n2 = _sut.ReadModel.Nodes.Last();
            _vm.AddEquipmentAtGpsLocation(EquipmentType.Terminal);
            _sut.Poller.Tick();
            _endTraceNode = _sut.ReadModel.Nodes.Last();
            _traceEquipment.Add(_sut.ReadModel.Equipments.Last().Id);
            _vm.AddFiber(_rtu.NodeId, n1.Id);
            _vm.AddFiber(n1.Id, _endTraceNode.Id);
            _vm.AddFiber(_rtu.NodeId, n2.Id);
            _sut.Poller.Tick();
        }

        [Given(@"Существует трасса от данного РТУ")]
        public void GivenСуществуетТрассаОтДанногоРТУ()
        {
            var traceNodes = new PathFinder(_sut.ReadModel).FindPath(_rtu.NodeId, _endTraceNode.Id).ToList();
            new AddTraceViewModel(_sut.ReadModel, _sut.Aggregate, traceNodes, _traceEquipment).Save();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь кликает на РТУ удалить")]
        public void WhenПользовательКликаетНаРТУУдалить()
        {
            _vm.RemoveRtu(_rtu.Id);
            _sut.Poller.Tick();
        }

        [Then(@"РТУ удаляется")]
        public void ThenРТУУдаляется()
        {
            _sut.ReadModel.Rtus.Count.Should().Be(0);
        }

        [Then(@"Узел под РТУ и присоединенные к нему отрезки удаляются")]
        public void ThenУзелПодРТУИПрисоединенныеКНемуОтрезкиУдаляются()
        {
            _sut.ReadModel.Nodes.FirstOrDefault(n => n.Id == _rtuNodeId).Should().Be(null);
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Node1 == _rtuNodeId || f.Node2 == _rtuNodeId).Should().BeNull();
        }

        [Then(@"Удаление РТУ не происходит")]
        public void ThenУдалениеРТУНеПроисходит()
        {
            _sut.ReadModel.Rtus.FirstOrDefault(r => r.Id == _rtu.Id).Should().NotBeNull();
        }
    }
}