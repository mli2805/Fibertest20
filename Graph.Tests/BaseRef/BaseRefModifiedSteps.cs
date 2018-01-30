using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Optixsoft.SorExaminer.OtdrDataFormat;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    

    [Binding]
    public sealed class BaseRefModifiedSteps
    {
        private SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private Iit.Fibertest.Graph.Trace _trace;
        private List<BaseRefDto> _baseRefs;

      
        [Given(@"Существует инициализированный RTU")]
        public void GivenСуществуетИнициализированныйRtu()
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            _rtu = _sut.ReadModel.Rtus.Last();

            _sut.InitializeRtu(_rtu.Id, @"1.1.1.1", "", @"SM1550");
        }

        [Given(@"К нему нарисована трасса1")]
        public void GivenКНемуНарисованаТрасса1()
        {
            var nodeForRtuId = _rtu.NodeId;

            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Cross, Latitude = 55.002, Longitude = 30.002 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var nodeIdA = _sut.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = nodeIdA }).Wait();

            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55.02, Longitude = 30.02}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            var nodeIdB = _sut.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();

            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55.04, Longitude = 30.04 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            nodeIdA = _sut.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();

            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55.06, Longitude = 30.06 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            nodeIdB = _sut.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();

            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Closure, Latitude = 55.08, Longitude = 30.08 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            nodeIdA = _sut.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();

            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55.082, Longitude = 30.082 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            nodeIdB = _sut.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();

            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.EmptyNode, Latitude = 55.084, Longitude = 30.084 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            nodeIdA = _sut.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();

            _sut.ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal, Latitude = 55.086, Longitude = 30.086 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
            nodeIdB = _sut.ReadModel.Nodes.Last().Id;
            _sut.ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeIdA, Node2 = nodeIdB }).Wait();
            _sut.Poller.EventSourcingTick().Wait();

            _trace =  _sut.DefineTrace(nodeIdB, nodeForRtuId, @"Трасса1", 3);
        }

        [When(@"Пользователь задает рефлектограмму")]
        public void WhenПользовательЗадаетРефлектограмму()
        {
            var vm = _sut.Container.Resolve<BaseRefsAssignViewModel>();
            vm.Initialize(_trace);
            vm.PreciseBaseFilename = SystemUnderTest.Base1550Lm4YesThresholds;
            _baseRefs = vm.GetBaseRefChangesList();
        }

        [Then(@"Перед применением рефлектограмма изменена")]
        public void ThenПередПрименениемРефлектограммаИзменена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));

            var baseRefChecker = _sut.Container.Resolve<BaseRefsChecker>();
            baseRefChecker.IsBaseRefsAcceptable(_baseRefs, _trace).Should().BeTrue();

            SorData.TryGetFromBytes(_baseRefs[0].SorBytes, out var otdrKnownBlocks);
            otdrKnownBlocks.LinkParameters.LandmarkBlocks.Length.Should().Be(9);
            var landmark = otdrKnownBlocks.LinkParameters.LandmarkBlocks[2];
            landmark.Code.Should().Be(LandmarkCode.Manhole);
            landmark.Location.Should().Be(114849);
        }

    }
}