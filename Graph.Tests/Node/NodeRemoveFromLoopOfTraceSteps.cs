using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Dto;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeRemoveFromLoopOfTraceSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Fiber _doublePassedFiber;
        private Iit.Fibertest.Graph.Trace _trace;
        private Guid _nodeId;
        private Guid _equipmentId;

        [Given(@"Задана трасса проходящая по волокну дважды")]
        public void GivenЗаданаТрассаПроходящаяПоВолокнуДважды()
        {
            _trace = _sut.CreateTraceDoublePassingClosure();
            _nodeId = _trace.NodeIds[3];

            var n1 = _trace.NodeIds[2];
            var n2 = _trace.NodeIds[3];
            _doublePassedFiber = _sut.ReadModel.Fibers.First(
                f => f.NodeId1 == n1 && f.NodeId2 == n2 ||
                     f.NodeId1 == n2 && f.NodeId2 == n1);
        }

        [When(@"Пользователь удаляет разворотный узел")]
        public void WhenПользовательУдаляетРазворотныйУзел()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.GrmNodeRequests.RemoveNode(_nodeId, EquipmentType.EmptyNode).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }
       
        [Then(@"Трасса укорачивается на два узла")]
        public void ThenТрассаУкорачиваетсяНаДваУзла()
        {
//            _trace.NodeIds.Count.Should().Be(4);
//            _trace.EquipmentIds.Count.Should().Be(4);

        }

        [Then(@"Дважды использованное волокно удаляется из графа")]
        public void ThenДваждыИспользованноеВолокноУдаляетсяИзГрафа()
        {

        }

    }
}
