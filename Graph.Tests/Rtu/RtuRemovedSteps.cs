using System.Linq;
using FluentAssertions;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuRemovedSteps
    {
        private readonly SutForRtuRemoved _sut = new SutForRtuRemoved();


        [Given(@"Существуют РТУ несколько узлов и отрезки между ними")]
        public void GivenСуществуютРтуНесколькоУзловИОтрезкиМеждуНими()
        {
            _sut.CreateRtuAndFewNodesAndFibers();
        }


        [Given(@"Существует трасса от этого РТУ")]
        public void GivenСуществуетТрассаОтЭтогоРту()
        {
            _sut.CreateTrace();
        }

        [Given(@"Трасса присоединенна к порту РТУ")]
        public void GivenТрассаПрисоединеннаКПортуРту()
        {
            _sut.AttachTrace(_sut.TraceId);
        }

        [When(@"Пользователь кликает на РТУ удалить")]
        public void WhenПользовательКликаетНаРтуУдалить()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestRemoveRtu() {NodeId = _sut.RtuNodeId}).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Трассы очищаются")]
        public void ThenТрассыОчищаются()
        {
            _sut.ReadModel.Traces.FirstOrDefault(t => t.Id == _sut.TraceId).Should().BeNull();
            _sut.ShellVm.GraphReadModel.Traces.FirstOrDefault(t => t.Id == _sut.TraceId).Should().BeNull();
        }

        [Then(@"РТУ удаляется")]
        public void ThenРтуУдаляется()
        {
            _sut.ReadModel.Rtus.FirstOrDefault(r => r.NodeId == _sut.RtuNodeId).Should().BeNull();
        }

        [Then(@"Узел под РТУ и присоединенные к нему отрезки удаляются")]
        public void ThenУзелПодРтуиПрисоединенныеКНемуОтрезкиУдаляются()
        {
            _sut.ReadModel.Nodes.FirstOrDefault(n => n.Id == _sut.RtuNodeId).Should().Be(null);
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Node1 == _sut.RtuNodeId || f.Node2 == _sut.RtuNodeId).Should().BeNull();
        }

        [Then(@"У РТУ пункт меню Удалить недоступен")]
        public void ThenУртуПунктМенюУдалитьНедоступен()
        {
            var rtuId = _sut.ReadModel.Rtus.First(r => r.NodeId == _sut.RtuNodeId).Id;
            var rtuLeaf = _sut.ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(rtuId);
            var removeRtuItem = rtuLeaf.MyContextMenu.First(i => i?.Header == Resources.SID_Remove);

            var rtuVm = _sut.ShellVm.GraphReadModel.Rtus.First(r => r.Id == rtuId);
        }
    }
}