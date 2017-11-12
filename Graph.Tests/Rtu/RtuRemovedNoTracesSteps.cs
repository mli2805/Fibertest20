using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuRemovedNoTracesSteps
    {
        private readonly SutForRtuRemoved _sut = new SutForRtuRemoved();
        private Guid _rtuId;
        private RtuLeaf _rtuLeaf;

        [Given(@"Существует РТУ для удаления")]
        public void GivenСуществуетРтуДляУдаления()
        {
            _rtuId = _sut.CreateRtu();
            _rtuLeaf = (RtuLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.First(r => r.Id == _rtuId);
        }
        [Given(@"Существуют еще несколько узлов и отрезки между ними")]
        public void GivenСуществуютЕщеНесколькоУзловИОтрезкиМеждуНими()
        {
            _sut.CreateOneRtuAndFewNodesAndFibers();
        }

        [When(@"Пользователь кликает удалить этот RTU")]
        public void WhenПользовательКликаетУдалитьЭтотRtu()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestRemoveRtu() { NodeId = _sut.RtuANodeId }).Wait();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь кликает на RTU в дереве удалить")]
        public void WhenПользовательКликаетНаRtuвДеревеУдалить()
        {
            var menuItemVm = _rtuLeaf.MyContextMenu.FirstOrDefault(i => i?.Header == Resources.SID_Remove);
            menuItemVm?.Command.Execute(_rtuLeaf);
            _sut.Poller.Tick();
        }

        [Then(@"РТУ удаляется")]
        public void ThenРтуУдаляется()
        {
            _sut.ReadModel.Rtus.FirstOrDefault(r => r.Id == _rtuId).Should().BeNull();
            _sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.FirstOrDefault(r => r.Id == _rtuId).Should().BeNull();
            _sut.ShellVm.GraphReadModel.Rtus.FirstOrDefault(r => r.Id == _rtuId).Should().BeNull();
        }

        [Then(@"Узел под РТУ и присоединенные к нему отрезки удаляются")]
        public void ThenУзелПодРтуиПрисоединенныеКНемуОтрезкиУдаляются()
        {
            _sut.ReadModel.Nodes.FirstOrDefault(n => n.Id == _sut.RtuANodeId).Should().Be(null);
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.Node1 == _sut.RtuANodeId || f.Node2 == _sut.RtuANodeId)
                .Should().BeNull();
        }

    }
}