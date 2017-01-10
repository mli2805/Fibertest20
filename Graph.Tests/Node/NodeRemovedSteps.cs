using System;
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
        private Guid _fiberId;

        public NodeRemovedSteps()
        {
            _vm = new MapViewModel(_sut.Aggregate);
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
            var anotherNodeId = _sut.ReadModel.Nodes.Last().Id;
            _vm.AddFiber(_nodeId, anotherNodeId);
            _sut.Poller.Tick();
            _fiberId = _sut.ReadModel.Fibers.Last().Id;
        }

        [When(@"Пользователь кликает удалить узел")]
        public void WhenПользовательКликаетУдалитьУзел()
        {
            _vm.RemoveNode(_nodeId);
            _sut.Poller.Tick();
        }

        [Then(@"Узел удаляется")]
        public void ThenУзелУдаляется()
        {
            _sut.ReadModel.Nodes.FirstOrDefault(n => n.Id == _nodeId).Should().Be(null);
        }

        [Then(@"Удаляется отрезок и узел")]
        public void ThenУдаляетсяОтрезокИУзел()
        {
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Id == _fiberId).Should().Be(null);
            _sut.ReadModel.Nodes.FirstOrDefault(n => n.Id == _nodeId).Should().Be(null);
        }
    }
}
