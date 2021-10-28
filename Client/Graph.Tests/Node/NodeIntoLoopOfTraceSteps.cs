using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeIntoLoopOfTraceSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Iit.Fibertest.Graph.Fiber _doublePassedFiber;
        private Iit.Fibertest.Graph.Trace _trace;
        private Guid _nodeId;
        private Guid _equipmentId;
        private int _nodeCount;

        [Given(@"Трасса проходит по волокну дважды")]
        public void GivenТрассаПроходитПоВолокнуДважды()
        {
            _trace = _sut.CreateTraceDoublePassingClosure();
            var n1 = _trace.NodeIds[2];
            var n2 = _trace.NodeIds[3];
            _doublePassedFiber =  _sut.ReadModel.Fibers.First(
                f => f.NodeId1 == n1 && f.NodeId2 == n2 ||
                     f.NodeId1 == n2 && f.NodeId2 == n1);
            _nodeCount = _trace.NodeIds.Count;
            _nodeCount.Should().Be(8);
        }

        [When(@"Пользователь добавляет в это волокно узел")]
        public void WhenПользовательДобавляетВЭтоВолокноУзел()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber() { FiberId = _doublePassedFiber.FiberId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _nodeId = _sut.ReadModel.Nodes.Last().NodeId;
            _equipmentId = _sut.ReadModel.Equipments.Last().EquipmentId;
        }

        [Then(@"Трасса удлиняется на два узла")]
        public void ThenТрассаУдлиняетсяНаДваУзла()
        {
            _trace.NodeIds.Count.Should().Be(_nodeCount+2);
            _trace.EquipmentIds.Count.Should().Be(_nodeCount + 2);

            _trace.NodeIds[3].Should().Be(_nodeId);
            _trace.NodeIds[7].Should().Be(_nodeId);

            _trace.EquipmentIds[3].Should().Be(_equipmentId);
            _trace.EquipmentIds[7].Should().Be(_equipmentId);
        }

     
    }
}
