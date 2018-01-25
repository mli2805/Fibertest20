using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    

    [Binding]
    public sealed class BaseRefModifiedSteps
    {
        private readonly BaseRefAdjuster _baseRefAdjuster;
        private SutForTraceAttach _sut = new SutForTraceAttach();
        private RtuLeaf _rtuLeaf;
        private Iit.Fibertest.Graph.Rtu _rtu;
        private TraceLeaf _traceLeaf;
        private Iit.Fibertest.Graph.Trace _trace;
        private List<BaseRefDto> _baseRefs;


        public BaseRefModifiedSteps(BaseRefAdjuster baseRefAdjuster)
        {
            _baseRefAdjuster = baseRefAdjuster;
        }

        [Given(@"Существует инициализированный RTU")]
        public void GivenСуществуетИнициализированныйRtu()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _rtu = _sut.ReadModel.Rtus.Last();
            _rtuLeaf = (RtuLeaf)_sut.ShellVm.TreeOfRtuModel.Tree.GetById(_rtu.Id);
            
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, _rtuLeaf.Id, @"1.1.1.1", "", @"SM1550", Answer.Yes));
            _sut.RtuLeafActions.InitializeRtu(_rtuLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"К нему нарисована трасса1")]
        public void GivenКНемуНарисованаТрасса1()
        {
            var nodeForRtuId = _rtu.NodeId;

            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55.1, Longitude = 30.1}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var firstNodeId = _sut.ReadModel.Nodes.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal, Latitude = 55.3, Longitude = 30.7 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var secondNodeId = _sut.ReadModel.Nodes.Last().Id;

            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = firstNodeId }).Wait();
            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = firstNodeId, Node2 = secondNodeId }).Wait();
            _sut.Poller.EventSourcingTick().Wait();

            _trace =  _sut.DefineTrace(secondNodeId, nodeForRtuId);
            _traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuModel.Tree.GetById(_trace.Id);
        }

        [When(@"Пользователь задает рефлектограмму")]
        public void WhenПользовательЗадаетРефлектограмму()
        {
            var vm = _sut.Container.Resolve<BaseRefsAssignViewModel>();
            vm.Initialize(_trace);
            vm.PreciseBaseFilename = @"..\..\Sut\BaseRefs\base1550-2lm-3-thresholds.sor";
            _baseRefs = vm.GetBaseRefChangesList();
        }

        [Then(@"Перед применением рефлектограмма изменена")]
        public void ThenПередПрименениемРефлектограммаИзменена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));

            var baseRefChecker = _sut.Container.Resolve<BaseRefsChecker>();
            baseRefChecker.IsBaseRefsAcceptable(_baseRefs, _trace).Should().BeTrue();

            SorData.TryGetFromBytes(_baseRefs[0].SorBytes, out var otdrKnownBlocks);
            otdrKnownBlocks.KeyEvents.KeyEvents.Length.Should().Be(3);
            var keyEvent = otdrKnownBlocks.KeyEvents.KeyEvents[1];
            keyEvent.Comment.Should().Be("111");
        }

    }
}