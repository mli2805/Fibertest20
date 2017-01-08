using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeIntoFiberAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private readonly MapViewModel _vm;
        private Guid _nodeForRtuId;
        private Guid _firstNodeId;
        private Guid _nodeId;
        private Guid _fiberId;
        private Guid _traceId;
        private int _cutOff;

        public NodeIntoFiberAddedSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate);
        }

        [Given(@"Есть трасса")]
        public void GivenЕстьТрасса()
        {
            _vm.AddRtuAtGpsLocation();
            _sut.Poller.Tick();
            _nodeForRtuId = _sut.ReadModel.Nodes.Single().Id;
            _vm.AddNode();
            _vm.AddNode();
            _sut.Poller.Tick();
            _firstNodeId = _sut.ReadModel.Nodes[1].Id;
            var secondNodeId = _sut.ReadModel.Nodes.Last().Id;
            _vm.AddFiber(_nodeForRtuId, _firstNodeId);
            _vm.AddFiber(_firstNodeId, secondNodeId);
            _sut.Poller.Tick();
            _fiberId = _sut.ReadModel.Fibers.First().Id;
            _traceId = Guid.NewGuid();
            var cmd = new AddTrace() {Id = _traceId, Nodes = { _nodeForRtuId, _firstNodeId, secondNodeId}};
            _sut.AddTrace(cmd);
            _sut.Poller.Tick();
        }

        [When(@"Пользователь кликает добавить узел в отрезок этой трассы")]
        public void WhenПользовательКликаетДобавитьУзелВОтрезок()
        {
            _vm.AddNodeIntoFiber(_fiberId);
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
        }

        [Then(@"Старый отрезок удаляется и добавляются два новых и новый узел связывает их")]
        public void ThenВместоОтрезкаОбразуетсяДваНовыхИНовыйУзелСвязывающийИх()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Id == _fiberId).Should().Be(null);
            _sut.ReadModel.Fibers.FirstOrDefault(f => new NodePairKey(f.Node1, f.Node2).
                        Equals(new NodePairKey(_firstNodeId,_nodeId))).Should().NotBe(null);
            _sut.ReadModel.Fibers.FirstOrDefault(f => new NodePairKey(f.Node1, f.Node2).
                        Equals(new NodePairKey(_nodeForRtuId, _nodeId))).Should().NotBe(null);
        }

        [Then(@"Новый узел входит в трассу")]
        public void ThenНовыйУзелВходитВТрассуАСвязностьТрассыСохраняется()
        {
            var trace = _sut.ReadModel.Traces.Single(t => t.Id == _traceId);
            trace.Nodes.Should().Contain(_nodeId);

        }

    }
}
