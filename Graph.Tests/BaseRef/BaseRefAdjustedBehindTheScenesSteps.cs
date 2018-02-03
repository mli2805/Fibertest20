using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class BaseRefAdjustedBehindTheScenesSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private Iit.Fibertest.Graph.Trace _trace;
        private TraceLeaf _traceLeaf;

        private TraceStatisticsViewModel _vm;
        private string _assignedBy;
        private DateTime _assignedAt;
        private int _closureLocation;

        [Given(@"Существует трасса с заданной базовой")]
        public void GivenСуществуетТрассаСЗаданнойБазовой()
        {
            _rtu = _sut.SetInitializedRtu();
            _trace = _sut.SetTrace(_rtu.NodeId, @"Trace1");
            _traceLeaf = (TraceLeaf) _sut.ShellVm.TreeOfRtuModel.Tree.GetById(_trace.Id);
            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1550Lm4YesThresholds, null, null, Answer.Yes);
        }

        [When(@"Пользователь открывает статистику по трассе")]
        public async void WhenПользовательОткрываетСтатистикуПоТрассе()
        {
            _vm = _sut.Container.Resolve<TraceStatisticsViewModel>();
            await _vm.Initialize(_trace.Id);
        }

        [Then(@"Там есть строка для базовой с именем и временем задания")]
        public async void ThenТамЕстьСтрокаДляБазовойСИменемИВременемЗадания()
        {
            var line = _vm.BaseRefs.First(i => i.BaseRefType == BaseRefType.Precise);
            line.Should().NotBe(null);
            _assignedBy = line.AssignedBy;
            _assignedAt = line.AssignedAt;

            var wcf = _sut.Container.Resolve<IWcfServiceForClient>();
            var baseRefs = await wcf.GetTraceBaseRefsAsync(_trace.Id);
            baseRefs.Count.Should().Be(1);

            var otdrDataKnownBlocks = SorData.FromBytes(baseRefs[0].SorBytes);
            _closureLocation = otdrDataKnownBlocks.LinkParameters.LandmarkBlocks[3].Location;

            _vm.TryClose();
        }

        [When(@"Пользователь сдвигает узел трассы")]
        public void WhenПользовательСдвигаетУзелТрассы()
        {
            var nodeId = _trace.Nodes[3];
            var node = _sut.ReadModel.Nodes.First(n => n.Id == nodeId);
            node.Latitude = 40;
            node.Longitude = 20;
            _sut.ShellVm.ComplyWithRequest(new MoveNode()
            {
                NodeId = nodeId,
                Latitude = 40,
                Longitude = 20
            }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Снова открывает статистику")]
        public async void WhenСноваОткрываетСтатистику()
        {
            _vm = _sut.Container.Resolve<TraceStatisticsViewModel>();
            await _vm.Initialize(_trace.Id);
        }

        [Then(@"Имя и время пользователя не изменились")]
        public void ThenИмяИВремяПользователяНеИзменились()
        {
            var line = _vm.BaseRefs.First(i => i.BaseRefType == BaseRefType.Precise);
            line.Should().NotBe(null);
            line.AssignedBy.Should().Be(_assignedBy);
            line.AssignedAt.Should().Be(_assignedAt);
            _vm.TryClose();
        }

        [Then(@"Изменилось положение ориентиров")]
        public async void ThenИзменилосьПоложениеОриентиров()
        {
            var wcf = _sut.Container.Resolve<IWcfServiceForClient>();
            var baseRefs = await wcf.GetTraceBaseRefsAsync(_trace.Id);
            baseRefs.Count.Should().Be(1);

            var otdrDataKnownBlocks = SorData.FromBytes(baseRefs[0].SorBytes);
            otdrDataKnownBlocks.LinkParameters.LandmarkBlocks[3].Location.Should().NotBe(_closureLocation);
        }
    }
}