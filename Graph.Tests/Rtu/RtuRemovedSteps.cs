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
        private readonly SutForRtuRemoved _sut = new SutForRtuRemoved();
        private Guid _rtuId;
        private RtuLeaf _rtuLeaf;
        private Guid _traceId;

        [Given(@"Существует один RTU")]
        public void GivenСуществуетОдинRtu()
        {
            _rtuId = _sut.CreateRtu();
            _rtuLeaf = (RtuLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.First(r => r.Id == _rtuId);

        }

        [Given(@"Существуют еще RTU несколько узлов и отрезки между ними")]
        public void GivenСуществуютЕщеRtuНесколькоУзловИОтрезкиМеждуНими()
        {
            _sut.CreateOneRtuAndFewNodesAndFibers();
        }

        [Given(@"Существует трасса от этого РТУ")]
        public void GivenСуществуетТрассаОтЭтогоРту()
        {
            _traceId = _sut.CreateTrace();
        }

        [Given(@"Существует трасса от второго РТУ последние отрезки трасс совпадают")]
        public void GivenСуществуетТрассаОтВторогоРтуПоследниеОтрезкиТрассСовпадают()
        {
            _sut.CreateAnotherTraceWhichInterceptedFirst();
        }

        [When(@"Пользователь кликает удалить первый RTU")]
        public void WhenПользовательКликаетУдалитьПервыйRtu()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestRemoveRtu() { NodeId = _sut.RtuANodeId }).Wait();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь кликает на первом RTU в дереве удалить")]
        public void WhenПользовательКликаетНаПервомRtuвДеревеУдалить()
        {
            var menuItemVm = _rtuLeaf.MyContextMenu.FirstOrDefault(i => i?.Header == Resources.SID_Remove);
            menuItemVm?.Command.Execute(null);
            _sut.Poller.Tick();
        }

        [Given(@"Трасса присоединенна к порту РТУ")]
        public void GivenТрассаПрисоединеннаКПортуРту()
        {
            _sut.InitializeRtu(_rtuId);
            _sut.AttachTraceTo(_traceId, _rtuLeaf, 2, Answer.Yes);
        }

       [Then(@"Трасса удаляемого RTU не удаляется но очищается")]
        public void ThenТрассаУдаляемогоRtuНеУдаляютсяНоОчищаются()
        {
            _sut.ReadModel.Traces.FirstOrDefault(t => t.Id == _traceId).Should().BeNull();
            _sut.ShellVm.GraphReadModel.Traces.FirstOrDefault(t => t.Id == _traceId).Should().BeNull();

            _sut.ShellVm.GraphReadModel.Fibers.FirstOrDefault(f => f.Id == _sut.Fiber1Id).Should().BeNull();
            _sut.ShellVm.GraphReadModel.Fibers.First(f => f.Id == _sut.Fiber2Id).State.Should()
                .Be(FiberState.NotInTrace);
            _sut.ShellVm.GraphReadModel.Fibers.First(f => f.Id == _sut.Fiber3Id).State.Should()
                .Be(FiberState.NotJoined);
        }


        [Then(@"У РТУ на карте пункт меню Удалить недоступен")]
        public void ThenУртуНаКартеПунктМенюУдалитьНедоступен()
        {
            //TODO check unavailability of Remove menu item on map
            //var rtuVm = _sut.ShellVm.GraphReadModel.Rtus.First(r => r.RtuId == rtuId);
        }

        [Then(@"У РТУ в дереве пункт меню Удалить недоступен")]
        public void ThenУртувДеревеПунктМенюУдалитьНедоступен()
        {
            var removeRtuItem = _rtuLeaf.MyContextMenu.First(i => i?.Header == Resources.SID_Remove);
            removeRtuItem.Command.CanExecute(null).Should().BeFalse();
        }
    }
}