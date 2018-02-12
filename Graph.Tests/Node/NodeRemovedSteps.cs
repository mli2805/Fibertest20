using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
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
            _nodeId = _sut.ReadModel.Nodes.Last().Id;
        }

        [Given(@"К данному узлу присоединен отрезок")]
        public void GivenКДанномуУзлуПрисоединенОтрезок()
        {
            _sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation()).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _anotherNodeId = _sut.ReadModel.Nodes.Last().Id;
            _sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { Node1 = _nodeId, Node2 = _anotherNodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Задана трасса")]
        public void GivenЗаданаТрасса()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _sut.InitializeRtu(_trace.RtuId);

            _rtuNodeId = _trace.Nodes[0];
            _lastNodeId = _trace.Nodes.Last();
        }

        [Given(@"Данный узел последний в трассе")]
        public void GivenДанныйУзелПоследнийВТрассе()
        {
            _nodeId = _trace.Nodes.Last();
        }

        [Given(@"Данный узел НЕ последний в трассе")]
        public void GivenДанныйУзелНеПоследнийВТрассе()
        {
            _nodeId = _trace.Nodes[1];
        }

        [Given(@"Для трассы задана базовая")]
        public void GivenДляТрассыЗаданаБазовая()
        {
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_trace.Id);
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
                    f.Node1 == _rtuNodeId && f.Node2 == _lastNodeId ||
                    f.Node1 == _lastNodeId && f.Node2 == _rtuNodeId);
            fiber.Should().NotBeNull();

            var fiberVm = _sut.GraphReadModel.Fibers.FirstOrDefault(f => f.Id == fiber?.Id);
            fiberVm.Should().NotBeNull();
            fiberVm?.States.Should().ContainKey(_trace.Id);
        }

        [Then(@"Корректируются списки узлов и оборудования трассы")]
        public void ThenКорректируютсяСпискиУзловИОборудованияТрассы()
        {
            _trace.Nodes.Contains(_nodeId).Should().BeFalse();
            _trace.Nodes.Count.Should().Be(_trace.Equipments.Count);

            var traceVm = _sut.GraphReadModel.Traces.First(t => t.Id == _trace.Id);
            for (int i = 0; i < traceVm.Nodes.Count - 1; i++)
            {
                _sut.GraphReadModel.Fibers.FirstOrDefault(
                    f =>
                        f.Node1.Id == traceVm.Nodes[i] && f.Node2.Id == traceVm.Nodes[i + 1] ||
                        f.Node1.Id == traceVm.Nodes[i + 1] && f.Node2.Id == traceVm.Nodes[i]).Should().NotBeNull();
            }
        }

        [Then(@"Отрезки связанные с исходным узлом удаляются")]
        public void ThenОтрезкиСвязанныеСИсходнымУзломУдаляются()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Node1 == _nodeId || f.Node2 == _nodeId).Should().Be(null);
        }

        [Then(@"Узел удаляется")]
        public void ThenУзелУдаляется()
        {
            _sut.ReadModel.Nodes.FirstOrDefault(n => n.Id == _nodeId).Should().Be(null);
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.NodeId == _nodeId).Should().Be(null);
        }

        [Then(@"Удаление не происходит")]
        public void ThenУдалениеНеПроисходит()
        {
            _trace.Nodes.Contains(_nodeId).Should().BeTrue();
        }
    }
}
