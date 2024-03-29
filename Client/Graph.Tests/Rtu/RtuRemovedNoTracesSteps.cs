﻿using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuRemovedNoTracesSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private RtuLeaf _rtuLeaf;

        [Given(@"Существует РТУ для удаления")]
        public void GivenСуществуетРтуДляУдаления()
        {
            _rtu = _sut.CreateRtuA();
            _rtuLeaf = (RtuLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.Tree.First(r => r.Id == _rtu.Id);
        }
        [Given(@"Существуют еще несколько узлов и отрезки между ними")]
        public void GivenСуществуютЕщеНесколькоУзловИОтрезкиМеждуНими()
        {
            _sut.CreateOneRtuAndFewNodesAndFibers(_rtu.NodeId);
        }

        [When(@"Пользователь кликает удалить этот RTU")]
        public void WhenПользовательКликаетУдалитьЭтотRtu()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.GrmRtuRequests.RemoveRtu(new RequestRemoveRtu() { NodeId = _rtu.NodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь кликает на RTU в дереве удалить")]
        public void WhenПользовательКликаетНаRtuвДеревеУдалить()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            var menuItemVm = _rtuLeaf.MyContextMenu.FirstOrDefault(i => i?.Header == Resources.SID_Remove);
            menuItemVm?.Command.Execute(_rtuLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"РТУ удаляется")]
        public void ThenРтуУдаляется()
        {
            _sut.ReadModel.Rtus.FirstOrDefault(r => r.Id == _rtu.Id).Should().BeNull();
            _sut.TreeOfRtuViewModel.TreeOfRtuModel.Tree.FirstOrDefault(r => r.Id == _rtu.Id).Should().BeNull();
        }

        [Then(@"Узел под РТУ и присоединенные к нему отрезки удаляются")]
        public void ThenУзелПодРтуиПрисоединенныеКНемуОтрезкиУдаляются()
        {
            _sut.ReadModel.Nodes.FirstOrDefault(n => n.NodeId == _rtu.NodeId).Should().Be(null);
            _sut.ReadModel.Fibers.FirstOrDefault(f => f.NodeId1 == _rtu.NodeId || f.NodeId2 == _rtu.NodeId)
                .Should().BeNull();
        }

    }
}