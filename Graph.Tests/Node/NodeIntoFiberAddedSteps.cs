using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeIntoFiberAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _nodeForRtuId;
        private Guid _firstNodeId;
        private Guid _nodeId;
        private Guid _fiberId;
        private Guid _traceId;
        private int _cutOff;


        [Given(@"Есть трасса")]
        public void GivenЕстьТрасса()
        {
            _nodeForRtuId = _sut.AddRtuAtGpsLocation();
            _firstNodeId = _sut.AddNode();
            var secondNodeId = _sut.AddNode();
            _fiberId = _sut.AddFiber(_nodeForRtuId, _firstNodeId);
            var fiberId2 = _sut.AddFiber(_firstNodeId, secondNodeId);
            _traceId = Guid.NewGuid();
            var cmd = new AddTrace() {Id = _traceId, Nodes = { _nodeForRtuId, _firstNodeId, secondNodeId}};
            _sut.AddTrace(cmd);

        }

        [When(@"Пользователь кликает добавить узел в отрезок этой трассы")]
        public void WhenПользовательКликаетДобавитьУзелВОтрезок()
        {
            _nodeId = _sut.AddNodeIntoFiber(_fiberId);
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
