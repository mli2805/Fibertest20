using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeRemoveFromLoopOfTraceSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginAsRoot(Answer.Yes);
        private Iit.Fibertest.Graph.Fiber _doublePassedFiber1, _doublePassedFiber2;
        private Iit.Fibertest.Graph.Trace _trace;
        private Guid _nodeId;
        private int _traceNodesCount;
        private int _allFibersInModel;

        [Given(@"Задана трасса проходящая по волокну дважды")]
        public void GivenЗаданаТрассаПроходящаяПоВолокнуДважды()
        {
            _trace = _sut.CreateTraceDoublePassingClosure();
            _traceNodesCount = _trace.NodeIds.Count;

            var n2 = _trace.NodeIds[2];
            var n3 = _trace.NodeIds[3];
            var n4 = _trace.NodeIds[4];

            _doublePassedFiber1 = _sut.ReadModel.Fibers.First(
                f => f.NodeId1 == n2 && f.NodeId2 == n3 ||
                     f.NodeId1 == n3 && f.NodeId2 == n2);

            _doublePassedFiber2 = _sut.ReadModel.Fibers.First(
                f => f.NodeId1 == n4 && f.NodeId2 == n3 ||
                     f.NodeId1 == n3 && f.NodeId2 == n4);

            _allFibersInModel = _sut.ReadModel.Fibers.Count;
            _allFibersInModel.Should().Be(5);
        }

        [When(@"Пользователь удаляет разворотный узел")]
        public void WhenПользовательУдаляетРазворотныйУзел()
        {
            _nodeId = _trace.NodeIds[4];
            _sut.GraphReadModel.GrmNodeRequests.RemoveNode(_nodeId, EquipmentType.EmptyNode).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь кликает удалить узел в котором повернули в петлю")]
        public void WhenПользовательКликаетУдалитьУзелВКоторомПовернулиВПетлю()
        {
            _nodeId = _trace.NodeIds[2];
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.GrmNodeRequests.RemoveNode(_nodeId, EquipmentType.EmptyNode).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }
        [When(@"И снова кликает удалить узел в котором повернули в петлю")]
        public void WhenИСноваКликаетУдалитьУзелВКоторомПовернулиВПетлю()
        {
            _nodeId = _trace.NodeIds[3]; // position in trace changed
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.GrmNodeRequests.RemoveNode(_nodeId, EquipmentType.EmptyNode).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }


        [When(@"Пользователь удаляет узел из этого участка")]
        public void WhenПользовательУдаляетУзелИзЭтогоУчастка()
        {
            _nodeId = _trace.NodeIds[3];
            _sut.GraphReadModel.GrmNodeRequests.RemoveNode(_nodeId, EquipmentType.EmptyNode).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Предложено удалить точки привязки или добавить узлы трасса не меняется узел не удаляется")]
        public void ThenПредложеноУдалитьТочкиПривязкиИлиДобавитьУзлыТрассаНеМеняетсяУзелНеУдаляется()
        {
            _trace.NodeIds.Count.Should().Be(_traceNodesCount);
            _trace.EquipmentIds.Count.Should().Be(_traceNodesCount);

            _sut.ReadModel.Nodes.FirstOrDefault(n => n.NodeId == _nodeId).Should().NotBeNull();
        }

        [When(@"Пользователь добавляет узел между удаляемым и точкой привязки")]
        public void WhenПользовательДобавляетУзелМеждуУдаляемымИТочкойПривязки()
        {
            var fiber = _sut.ReadModel.Fibers.First(f =>
                f.NodeId1 == _trace.NodeIds[1] && f.NodeId2 == _trace.NodeIds[2]
                || f.NodeId1 == _trace.NodeIds[2] && f.NodeId2 == _trace.NodeIds[1]);
            _sut.GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber() { FiberId = fiber.FiberId, InjectionType = EquipmentType.EmptyNode }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _traceNodesCount = _trace.NodeIds.Count;
            _allFibersInModel = _sut.ReadModel.Fibers.Count;
        }

        [Then(@"Трасса укорачивается на два узла")]
        public void ThenТрассаУкорачиваетсяНаДваУзла()
        {
            _trace.NodeIds.Count.Should().Be(_traceNodesCount - 2);
            _trace.EquipmentIds.Count.Should().Be(_traceNodesCount - 2);
            _trace.FiberIds.Count.Should().Be(_traceNodesCount - 3);

            _sut.ReadModel.Fibers.Count.Should().Be(_allFibersInModel - 1);
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
