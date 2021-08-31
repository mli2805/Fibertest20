using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Optixsoft.SorExaminer.OtdrDataFormat;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class BaseRefAdjustedBehindTheScenesSteps
    {
        private const string NodeNewTitle = @"Node with changes";
        private const string EquipmentNewTitle = @"Equipment with changes";

        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private Iit.Fibertest.Graph.Trace _trace;
        private TraceLeaf _traceLeaf;

        private TraceStatisticsViewModel _vm;
        private string _assignedBy;
        private DateTime _assignedAt;
        private int _closureLocation;

        [Given(@"Существует трасса с заданными базовыми")]
        public void GivenСуществуетТрассаСЗаданнымиБазовыми()
        {
            _rtu = _sut.SetInitializedRtu();
            _trace = _sut.SetTrace(_rtu.NodeId, @"Trace1");
            _traceLeaf = (TraceLeaf)_sut.TreeOfRtuModel.GetById(_trace.TraceId);
            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1550Lm4YesThresholds,
                SystemUnderTest.Base1550Lm4YesThresholds, null, Answer.Yes);
        }

        [When(@"Пользователь открывает статистику по трассе")]
        public void WhenПользовательОткрываетСтатистикуПоТрассе()
        {
            _vm = _sut.ClientScope.Resolve<TraceStatisticsViewModel>();
            _vm.Initialize(_trace.TraceId);
        }

        [Then(@"Там есть строки для каждой базовой с именем и временем задания")]
        public async void ThenТамЕстьСтрокиДляКаждойБазовойСИменемИВременемЗадания()
        {
            var line = _vm.BaseRefs.First(i => i.BaseRefType == BaseRefType.Precise);
            line.Should().NotBe(null);
            _assignedBy = line.AssignedBy;
            _assignedAt = line.AssignedAt;
            var line2 = _vm.BaseRefs.First(i => i.BaseRefType == BaseRefType.Fast);
            line2.Should().NotBe(null);
            _assignedBy = line2.AssignedBy;
            _assignedAt = line2.AssignedAt;

            var baseRefs = _sut.ReadModel.BaseRefs.Where(b => b.TraceId == _trace.TraceId).ToList();
            baseRefs.Count.Should().Be(2);

            var wcf = _sut.ServerScope.Resolve<IWcfServiceCommonC2D>();
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
            _vm = _sut.ClientScope.Resolve<TraceStatisticsViewModel>();
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
            baseRefs.Count.Should().Be(2);
            var sorBytes = await _sut.WcfServiceCommonC2D.GetSorBytes(baseRefs[0].SorFileId);
            var otdrDataKnownBlocks = SorData.FromBytes(sorBytes);

            otdrDataKnownBlocks.LinkParameters.LandmarkBlocks[3].Location.Should().NotBe(_closureLocation);
        }

        [When(@"Пользователь меняет название узла а также название и тип оборудования в узле для трассы")]
        public void WhenПользовательМеняетНазваниеУзлаАТакжеНазваниеИТипОборудованияВУзлеДляТрассы()
        {
            var nodeId = _trace.NodeIds[5];
            var equipmentId = _trace.EquipmentIds[5];
            var nodeUpdateViewModel = _sut.ClientScope.Resolve<NodeUpdateViewModel>();
            nodeUpdateViewModel.Initialize(nodeId);
            nodeUpdateViewModel.Title = NodeNewTitle;

            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentInfoViewModelHandler(model, Answer.Yes, EquipmentType.Cross, 0, 0, EquipmentNewTitle));

            var item = nodeUpdateViewModel.EquipmentsInNode.First(i => i.Id == equipmentId);
            item.Command = new UpdateEquipment() { EquipmentId = equipmentId };
            _sut.Poller.EventSourcingTick().Wait();
            nodeUpdateViewModel.Save();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Меняется тип и название ориентира")]
        public void ThenМеняетсяТипИНазваниеОриентира()
        {
            var sorFileId = _sut.ReadModel.BaseRefs.First(b => b.Id == _trace.PreciseId).SorFileId;
            var sorbBytes = _sut.WcfServiceCommonC2D.GetSorBytes(sorFileId).Result;
            var sorData = SorData.FromBytes(sorbBytes);

            sorData.LinkParameters.LandmarkBlocks[5].Comment.Should().Be($@"{NodeNewTitle} / {EquipmentNewTitle}");
            sorData.LinkParameters.LandmarkBlocks[5].Code.Should().Be(LandmarkCode.WiringCloset);
        }

    }
}