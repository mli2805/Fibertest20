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
            _vm.AddFiber(rtuNodeId, _anotherNodeId);
            _sut.Poller.Tick();
            var addTraceViewModel = new AddTraceViewModel(_sut.ReadModel, _sut.Aggregate, new List<Guid>() { rtuNodeId, _anotherNodeId, _nodeId });
            addTraceViewModel.Save();
            _sut.Poller.Tick();
            _trace = _sut.ReadModel.Traces.Last();
        }

        [When(@"Пользователь кликает удалить узел")]
        public void WhenПользовательКликаетУдалитьУзел()
        {
            _vm.RemoveNode(_nodeId);
            _sut.Poller.Tick();
        }

        [Then(@"Корректируются списки узлов и оборудования трассы")]
        public void ThenКорректируютсяСпискиУзловИОборудованияТрассы()
        {
            _trace.Nodes.Contains(_nodeId).Should().BeFalse();
        }

        [Then(@"Удаляется отрезок")]
        public void ThenУдаляетсяОтрезок()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Id == _fiberId).Should().Be(null);
        }

        [Then(@"Узел удаляется")]
        public void ThenУзелУдаляется()
        {
            _sut.ReadModel.Nodes.FirstOrDefault(n => n.Id == _nodeId).Should().Be(null);
        }
    }
}
