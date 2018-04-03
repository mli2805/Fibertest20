using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
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
            _traceLeaf = (TraceLeaf)_sut.TreeOfRtuModel.GetById(_trace.TraceId);
            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1550Lm4YesThresholds, null, null, Answer.Yes);
        }

        [When(@"Пользователь открывает статистику по трассе")]
        public void WhenПользовательОткрываетСтатистикуПоТрассе()
        {
            _vm = _sut.ClientContainer.Resolve<TraceStatisticsViewModel>();
            _vm.Initialize(_trace.TraceId);
        }

        [Then(@"Там есть строка для базовой с именем и временем задания")]
        public async void ThenТамЕстьСтрокаДляБазовойСИменемИВременемЗадания()
        {
            var line = _vm.BaseRefs.First(i => i.BaseRefType == BaseRefType.Precise);
            line.Should().NotBe(null);
            _assignedBy = line.AssignedBy;
            _assignedAt = line.AssignedAt;

            var baseRefs = _sut.ReadModel.BaseRefs.Where(b => b.TraceId == _trace.TraceId).ToList();
            baseRefs.Count.Should().Be(1);

            var wcf = _sut.ServerContainer.Resolve<IWcfServiceForClient>();
            var sorBytes = await wcf.GetSorBytes(baseRefs[0].SorFileId);

            var otdrDataKnownBlocks = SorData.FromBytes(sorBytes);
            _closureLocation = otdrDataKnownBlocks.LinkParameters.LandmarkBlocks[3].Location;

            _vm.TryClose();
        }

        [When(@"Пользователь сдвигает узел трассы")]
        public void WhenПользовательСдвигаетУзелТрассы()
        {
            var nodeId = _trace.NodeIds[3];
            _sut.GraphReadModel.GrmNodeRequests.MoveNode(new MoveNode()
            {
                NodeId = nodeId,
                Latitude = 55.059,
                Longitude = 30.059
            }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Снова открывает статистику")]
        public void WhenСноваОткрываетСтатистику()
        {
            _vm = _sut.ClientContainer.Resolve<TraceStatisticsViewModel>();
            _vm.Initialize(_trace.TraceId);
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
            var baseRefs = _sut.ReadModel.BaseRefs.Where(b => b.TraceId == _trace.TraceId).ToList();
            baseRefs.Count.Should().Be(1);
            var sorBytes = await _sut.WcfService.GetSorBytes(baseRefs[0].SorFileId);
            var otdrDataKnownBlocks = SorData.FromBytes(sorBytes);

            otdrDataKnownBlocks.LinkParameters.LandmarkBlocks[3].Location.Should().NotBe(_closureLocation);
        }
    }
}