using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class NodeRemovedSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _nodeId;
        private Guid _rtuNodeId;
        private Guid _anotherNodeId;
        private Guid _lastNodeId;
        private Iit.Fibertest.Graph.Trace _trace;


        [Given(@"Существует узел")]
        public void GivenСуществуетУзел()
        {
//            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation()).Wait();
            _sut.Poller.Tick();
            _nodeId = _sut.ReadModel.Nodes.First().Id;
        }

        [Given(@"К данному узлу присоединен отрезок")]
        public void GivenКДанномуУзлуПрисоединенОтрезок()
        {
            _sut.ShellVm.ComplyWithRequest(new AddNode()).Wait();
            _sut.Poller.Tick();
            _anotherNodeId = _sut.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = _nodeId, Node2 = _anotherNodeId }).Wait();
            _sut.Poller.Tick();
        }

        [Given(@"Задана трасса")]
        public void GivenЗаданаТрасса()
        {
            _sut.CreateTraceRtuEmptyTerminal();
            _trace = _sut.ReadModel.Traces.Last();
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
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, SystemUnderTest2.Path, SystemUnderTest2.Path, null, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new RequestAssignBaseRef() { TraceId = _trace.Id }).Wait();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь кликает удалить узел")]
        public void WhenПользовательКликаетУдалитьУзел()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestRemoveNode() { Id = _nodeId }).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Создается отрезок между соседними с данным узлами")]
        public void ThenСоздаетсяОтрезокМеждуСоседнимиСДаннымУзлами()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(
                f =>
                    f.Node1 == _rtuNodeId && f.Node2 == _lastNodeId ||
                    f.Node1 == _lastNodeId && f.Node2 == _rtuNodeId).Should().NotBe(null);
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
            _sut.ReadModel.Equipments.FirstOrDefault(e => e.NodeId == _nodeId).Should().Be(null);
        }

        [Then(@"Удаление не происходит")]
        public void ThenУдалениеНеПроисходит()
        {
            _trace.Nodes.Contains(_nodeId).Should().BeTrue();
        }
    }
}
