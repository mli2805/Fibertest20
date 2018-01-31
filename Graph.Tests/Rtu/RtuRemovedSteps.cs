using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuRemovedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private RtuLeaf _rtuLeaf;
        private Guid _traceId;
        private Guid[] _fibers;
        private Guid[] _nodes;

        [Given(@"Существует один RTU")]
        public void GivenСуществуетОдинRtu()
        {
            _rtu = _sut.CreateRtuA();
            _rtuLeaf = (RtuLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.First(r => r.Id == _rtu.Id);
        }

        [Given(@"Существуют еще RTU несколько узлов и отрезки между ними")]
        public void GivenСуществуютЕщеRtuНесколькоУзловИОтрезкиМеждуНими()
        {
            _nodes = _sut.CreateOneRtuAndFewNodesAndFibers(_rtu.NodeId);
        }

        [Given(@"Существует трасса от первого РТУ")]
        public void GivenСуществуетТрассаОтЭтогоРту()
        {
            _traceId = _sut.DefineTrace(_nodes[2], _rtu.NodeId, @"title", 2).Id;
            _fibers = _sut.ReadModel.GetTraceFibers(_sut.ReadModel.Traces.FirstOrDefault(t => t.Id == _traceId)).Select(f=>f.Id).ToArray();
        }

        [Given(@"Существует трасса от второго РТУ последние отрезки трасс совпадают")]
        public void GivenСуществуетТрассаОтВторогоРтуПоследниеОтрезкиТрассСовпадают()
        {
            _sut.DefineTrace(_nodes[1], _nodes[0], @"title", 2);
        }

        [When(@"Пользователь кликает удалить первый RTU")]
        public void WhenПользовательКликаетУдалитьПервыйRtu()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestRemoveRtu() { NodeId = _rtu.NodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь кликает на первом RTU в дереве удалить")]
        public void WhenПользовательКликаетНаПервомRtuвДеревеУдалить()
        {
            _rtuLeaf.MyContextMenu.FirstOrDefault(i => i?.Header == Resources.SID_Remove)?.Command.Execute(_rtuLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Трасса присоединенна к порту РТУ")]
        public void GivenТрассаПрисоединеннаКПортуРту()
        {
            _sut.InitializeRtu(_rtu.Id);
            _sut.AttachTraceTo(_traceId, _rtuLeaf, 2, Answer.Yes);
        }

       [Then(@"Трасса удаляемого RTU не удаляется но очищается")]
        public void ThenТрассаУдаляемогоRtuНеУдаляютсяНоОчищаются()
        {
            _sut.ReadModel.Traces.FirstOrDefault(t => t.Id == _traceId).Should().BeNull();
            _sut.ShellVm.GraphReadModel.Traces.FirstOrDefault(t => t.Id == _traceId).Should().BeNull();

            _sut.ShellVm.GraphReadModel.Fibers.FirstOrDefault(f => f.Id == _fibers[0]).Should().BeNull();
            _sut.ShellVm.GraphReadModel.Fibers.First(f => f.Id == _fibers[1]).State.Should()
                .Be(FiberState.NotInTrace);
            _sut.ShellVm.GraphReadModel.Fibers.First(f => f.Id == _fibers[2]).State.Should()
                .Be(FiberState.NotJoined);
        }

        [Then(@"У РТУ на карте пункт меню Удалить недоступен")]
        public void ThenУртуНаКартеПунктМенюУдалитьНедоступен()
        {
            //TODO check unavailability of Remove menu item on map
            // _sut.ShellVm.MapUserControl - only in code behind
        }

        [Then(@"У РТУ в дереве пункт меню Удалить недоступен")]
        public void ThenУртувДеревеПунктМенюУдалитьНедоступен()
        {
            var removeRtuItem = _rtuLeaf.MyContextMenu.First(i => i?.Header == Resources.SID_Remove);
            removeRtuItem.Command.CanExecute(null).Should().BeFalse();
        }
    }
}