using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeRemovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private readonly MapViewModel _vm;
        private Guid _nodeId;
        private Guid _rtuNodeId;
        private Guid _anotherNodeId;
        private Guid _fiberId;
        private Iit.Fibertest.Graph.Trace _trace;

        public NodeRemovedSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate, _sut.ReadModel);
        }

        [Given(@"Существует узел")]
        public void GivenСуществуетУзел()
        {
            _vm.AddNode();
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.First().Id;
        }

        [Given(@"К данному узлу присоединен отрезок")]
        public void GivenКДанномуУзлуПрисоединенОтрезок()
        {
            _vm.AddNode();
            _sut.Poller.Tick();
            _anotherNodeId = _sut.ReadModel.Nodes.Last().Id;
            _vm.AddFiber(_nodeId, _anotherNodeId);
            _sut.Poller.Tick();
            _fiberId = _sut.ReadModel.Fibers.Last().Id;
        }

        [Given(@"Данный узел последний в трассе")]
        public void GivenДанныйУзелПоследнийВТрассе()
        {
            _vm.AddRtuAtGpsLocation();
            _sut.Poller.Tick();
            var rtuNodeId = _sut.ReadModel.Nodes.Last().Id;
            var rtuId = _sut.ReadModel.Rtus.Last().Id;
            _vm.AddFiber(rtuNodeId, _anotherNodeId);
            _sut.Poller.Tick();
            new AddEquipmentViewModel(_nodeId, _sut.ReadModel, _sut.Aggregate).Save();
            _sut.Poller.Tick();
            var equipmentId = _sut.ReadModel.Equipments.Last().Id;

            var nodes = new List<Guid>() { rtuNodeId, _anotherNodeId, _nodeId };
            var equipments = new List<Guid>() {rtuId, Guid.Empty, equipmentId };

            var addTraceViewModel = new AddTraceViewModel(_sut.ReadModel, _sut.Aggregate, nodes, equipments);
            addTraceViewModel.Save();
            _sut.Poller.Tick();
            _trace = _sut.ReadModel.Traces.Last();
        }

        [Given(@"Данный узел НЕ последний в трассе")]
        public void GivenДанныйУзелНЕПоследнийВТрассе()
        {
            _vm.AddRtuAtGpsLocation();
            _sut.Poller.Tick();
            _rtuNodeId = _sut.ReadModel.Nodes.Last().Id;
            var rtuId = _sut.ReadModel.Rtus.Last().Id;
            _vm.AddFiber(_rtuNodeId, _nodeId);
            _sut.Poller.Tick();
            new AddEquipmentViewModel(_anotherNodeId, _sut.ReadModel, _sut.Aggregate).Save();
            _sut.Poller.Tick();
            var equipmentId = _sut.ReadModel.Equipments.Last().Id;

            var nodes = new List<Guid>() { _rtuNodeId, _nodeId, _anotherNodeId };
            var equipments = new List<Guid>() { rtuId, Guid.Empty, equipmentId };

            var addTraceViewModel = new AddTraceViewModel(_sut.ReadModel, _sut.Aggregate, nodes, equipments);
            addTraceViewModel.Save();
            _sut.Poller.Tick();
            _trace = _sut.ReadModel.Traces.Last();
        }

        [Given(@"Для трассы задана базовая")]
        public void GivenДляТрассыЗаданаБазовая()
        {
            _trace.PreciseId = Guid.NewGuid();
        }

        [When(@"Пользователь кликает удалить узел")]
        public void WhenПользовательКликаетУдалитьУзел()
        {
            _vm.RemoveNode(_nodeId);
            _sut.Poller.Tick();
        }

        [Then(@"Создается отрезок между соседними с данным узлами")]
        public void ThenСоздаетсяОтрезокМеждуСоседнимиСДаннымУзлами()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(
                f =>
                    f.Node1 == _rtuNodeId && f.Node2 == _anotherNodeId ||
                    f.Node1 == _anotherNodeId && f.Node2 == _rtuNodeId).Should().NotBe(null);
        }

        [Then(@"Корректируются списки узлов и оборудования трассы")]
        public void ThenКорректируютсяСпискиУзловИОборудованияТрассы()
        {
            _trace.Nodes.Contains(_nodeId).Should().BeFalse();
            _trace.Nodes.Count.Should().Be(_trace.Equipments.Count);
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
        }

        [Then(@"Удаление не происходит")]
        public void ThenУдалениеНеПроисходит()
        {
            _trace.Nodes.Contains(_nodeId).Should().BeTrue();
        }
    }
}
