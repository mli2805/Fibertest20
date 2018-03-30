using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeRemovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _nodeId;
        private const EquipmentType Type = EquipmentType.Closure;
        private Guid _rtuNodeId;
        private Guid _anotherNodeId;
        private Guid _lastNodeId;
        private Iit.Fibertest.Graph.Trace _trace;


        [Given(@"Существует узел")]
        public void GivenСуществуетУзел()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation(){Type = Type}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _nodeId = _sut.ReadModel.Nodes.Last().NodeId;
        }

        [Given(@"К данному узлу присоединен отрезок")]
        public void GivenКДанномуУзлуПрисоединенОтрезок()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation()).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _anotherNodeId = _sut.ReadModel.Nodes.Last().NodeId;
            _sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = _nodeId, NodeId2 = _anotherNodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Задана трасса")]
        public void GivenЗаданаТрасса()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _sut.InitializeRtu(_trace.RtuId);

            _rtuNodeId = _trace.NodeIds[0];
            _lastNodeId = _trace.NodeIds.Last();
        }

        [Given(@"Данный узел последний в трассе")]
        public void GivenДанныйУзелПоследнийВТрассе()
        {
            _nodeId = _trace.NodeIds.Last();
        }

        [Given(@"Данный узел НЕ последний в трассе")]
        public void GivenДанныйУзелНеПоследнийВТрассе()
        {
            _nodeId = _trace.NodeIds[1];
        }

        [Given(@"Для трассы задана базовая")]
        public void GivenДляТрассыЗаданаБазовая()
        {
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            _sut.AssignBaseRef(traceLeaf, SystemUnderTest.Base1625Lm3, SystemUnderTest.Base1625Lm3, null, Answer.Yes);
        }

        [When(@"Пользователь кликает удалить узел")]
        public void WhenПользовательКликаетУдалитьУзел()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));

            _sut.GraphReadModel.GrmNodeRequests.RemoveNode(_nodeId, Type).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Создается отрезок между соседними с данным узлами")]
        public void ThenСоздаетсяОтрезокМеждуСоседнимиСДаннымУзлами()
        {
            var fiber = _sut.ReadModel.Fibers.FirstOrDefault(f =>
                    f.NodeId1 == _rtuNodeId && f.NodeId2 == _lastNodeId ||
                    f.NodeId1 == _lastNodeId && f.NodeId2 == _rtuNodeId);
            fiber.Should().NotBeNull();

            var fiberVm = _sut.GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == fiber?.FiberId);
            fiberVm.Should().NotBeNull();
            fiberVm?.States.Should().ContainKey(_trace.TraceId);
        }

        [Then(@"Корректируются списки узлов и оборудования трассы")]
        public void ThenКорректируютсяСпискиУзловИОборудованияТрассы()
        {
            _trace.NodeIds.Contains(_nodeId).Should().BeFalse();
            _trace.NodeIds.Count.Should().Be(_trace.EquipmentIds.Count);

            var trace = _sut.ReadModel.Traces.First(t => t.TraceId == _trace.TraceId);
            for (int i = 0; i < trace.NodeIds.Count - 1; i++)
            {
                _sut.ReadModel.Fibers.FirstOrDefault(
                    f =>
                        f.NodeId1 == trace.NodeIds[i] && f.NodeId2 == trace.NodeIds[i + 1] ||
                        f.NodeId1 == trace.NodeIds[i + 1] && f.NodeId2 == trace.NodeIds[i]).Should().NotBeNull();
            }
        }

        [Then(@"Отрезки связанные с исходным узлом удаляются")]
        public void ThenОтрезкиСвязанныеСИсходнымУзломУдаляются()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.NodeId1 == _nodeId || f.NodeId2 == _nodeId).Should().Be(null);
        }

        [Then(@"Узел удаляется")]
        public void ThenУзелУдаляется()
        {
            _sut.ReadModel.Nodes.FirstOrDefault(n => n.NodeId == _nodeId).Should().Be(null);
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.NodeId == _nodeId).Should().Be(null);
        }

        [Then(@"Удаление не происходит")]
        public void ThenУдалениеНеПроисходит()
        {
            _trace.NodeIds.Contains(_nodeId).Should().BeTrue();
        }
    }
}
