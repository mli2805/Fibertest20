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
        private Iit.Fibertest.Graph.Fiber _doublePassedFiber1, _doublePassedFiber2;
        private Iit.Fibertest.Graph.Trace _trace;
        private Guid _nodeId;

        [Given(@"Задана трасса проходящая по волокну дважды")]
        public void GivenЗаданаТрассаПроходящаяПоВолокнуДважды()
        {
            _trace = _sut.CreateTraceDoublePassingClosure();

            var n2 = _trace.NodeIds[2];
            var n3 = _trace.NodeIds[3];
            var n4 = _trace.NodeIds[4];

            _doublePassedFiber1 = _sut.ReadModel.Fibers.First(
                f => f.NodeId1 == n2 && f.NodeId2 == n3 ||
                     f.NodeId1 == n3 && f.NodeId2 == n2);

            _doublePassedFiber2 = _sut.ReadModel.Fibers.First(
                f => f.NodeId1 == n4 && f.NodeId2 == n3 ||
                     f.NodeId1 == n3 && f.NodeId2 == n4);
        }

        [When(@"Пользователь удаляет разворотный узел")]
        public void WhenПользовательУдаляетРазворотныйУзел()
        {
            _nodeId = _trace.NodeIds[4];
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.GrmNodeRequests.RemoveNode(_nodeId, EquipmentType.EmptyNode).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь удаляет узел в котором повернули в петлю")]
        public void WhenПользовательУдаляетУзелВКоторомПовернулиВПетлю()
        {
            _nodeId = _trace.NodeIds[2];
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.GrmNodeRequests.RemoveNode(_nodeId, EquipmentType.EmptyNode).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь удаляет узел из этого участка")]
        public void WhenПользовательУдаляетУзелИзЭтогоУчастка()
        {
            _nodeId = _trace.NodeIds[3];
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.GrmNodeRequests.RemoveNode(_nodeId, EquipmentType.EmptyNode).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }


        [Then(@"Трасса укорачивается на два узла")]
        public void ThenТрассаУкорачиваетсяНаДваУзла()
        {
            _trace.NodeIds.Count.Should().Be(6);
            _trace.EquipmentIds.Count.Should().Be(6);
        }

        [Then(@"Ближнее дважды использованное волокно удаляется из графа")]
        public void ThenБлижнееДваждыИспользованноеВолокноУдаляетсяИзГрафа()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.FiberId == _doublePassedFiber1.FiberId).Should().BeNull();
        }

        [Then(@"Дальнее дважды использованное волокно удаляется из графа")]
        public void ThenДальнееДваждыИспользованноеВолокноУдаляетсяИзГрафа()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.FiberId == _doublePassedFiber2.FiberId).Should().BeNull();
        }

    
    }
}
