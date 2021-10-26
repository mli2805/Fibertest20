using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class MoniResultComesSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginAsRoot();
        private Iit.Fibertest.Graph.Trace _trace;
        private TraceLeaf _traceLeaf;

        private NodeVm _crossVm;
        private NodeVm _closureVm;

        [Given(@"Трасса c 4 ориентирами на мониторинге")]
        public void GivenТрассаCОриентирамиНаМониторинге()
        {
            _trace = _sut.SetTraceWithAccidentInOldNode();
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(9);
            _sut.ReadModel.Nodes.Count.Should().Be(9);

            _crossVm = _sut.GraphReadModel.Data.Nodes.First(n => n.Id == _trace.NodeIds[1]);
            _closureVm = _sut.GraphReadModel.Data.Nodes.First(n => n.Id == _trace.NodeIds[5]);

            _trace.State.Should().Be(FiberState.NotJoined);
            _sut.AssertTraceFibersState(_trace);

            _traceLeaf = _sut.Attach(_trace, 3);

            _trace.State.Should().Be(FiberState.Unknown);
            _sut.AssertTraceFibersState(_trace);
        }

        [When(@"Приходит (.*) (.*) с файлом (.*)")]
        public void WhenПриходитFastFiberBreakСФайломTrace4LmBreakBnode2(string basetype, string seriousness, string filename)
        {
            var baseType = (BaseRefType)Enum.Parse(typeof(BaseRefType), basetype);
            var traceState = (FiberState)Enum.Parse(typeof(FiberState), seriousness);
            var sorBytes = File.ReadAllBytes($@"..\..\Sut\MoniResults\{filename}.sor");
            var dto = new MonitoringResultDto()
            {
                RtuId = _trace.RtuId,
                PortWithTrace = new PortWithTraceDto() { TraceId = _trace.TraceId, OtauPort = new OtauPortDto()},
                TraceState = traceState,
                BaseRefType = baseType,
                SorBytes = sorBytes,
            };
            _sut.MsmqMessagesProcessor.ProcessMonitoringResult(dto).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Отсоединяем трассу от порта")]
        public void WhenОтсоединяемТрассуОтПорта()
        {
            var menuItemVm = _traceLeaf.MyContextMenu.First(i => i?.Header == Resources.SID_Unplug_trace);
            menuItemVm.Command.Execute(_traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Добавляем на трассе точки привязки")]
        public void WhenДобавляемНаТрассеТочкиПривязки()
        {
            _sut.AddAdjustmentPoints(_trace);
        }


        [When(@"Присоединяем трассу к любому порту")]
        public void WhenПрисоединяемТрассуКЛюбомуПорту()
        {
            _traceLeaf = _sut.Attach(_trace, 2);
        }

        [Then(@"Все красное и Крест совпадающий с муфтой")]
        public void ThenВсеКрасноеИКрестСовпадающийСоВторойМуфтой()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(10);
            _sut.GraphReadModel.Data.Nodes.Count(n => n.Type == EquipmentType.AccidentPlace).Should().Be(1);
            var accidentPlaceNodeVm = _sut.GraphReadModel.Data.Nodes.First(n => n.Type == EquipmentType.AccidentPlace);
            accidentPlaceNodeVm.AccidentOnTraceVmId.Should().Be(_trace.TraceId);

            _sut.ReadModel.Nodes.Count.Should().Be(10);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(1);
            var accidentPlaceNode = _sut.GraphReadModel.Data.Nodes.First(n => n.Type == EquipmentType.AccidentPlace);
            accidentPlaceNode.AccidentOnTraceVmId.Should().Be(_trace.TraceId);

            _trace.State.Should().Be(FiberState.FiberBreak);
            _sut.AssertTraceFibersState(_trace);

            accidentPlaceNode.Position.ShouldBeEquivalentTo(_closureVm.Position);
            accidentPlaceNodeVm.Position.ShouldBeEquivalentTo(_closureVm.Position);
        }


        [Then(@"Все фиолетовое и Кресты совпадающие с проключением и муфтой")]
        public void ThenВсеФиолетовоеИКрестыСовпадающиеСПервойИВторойМуфтой()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(11);
            _sut.GraphReadModel.Data.Nodes.Count(n => n.Type == EquipmentType.AccidentPlace).Should().Be(2);
            var accidentPlaceNodeVm1 = _sut.GraphReadModel.Data.Nodes
                .First(n=>n.State == FiberState.Minor && n.Type == EquipmentType.AccidentPlace);
            accidentPlaceNodeVm1.AccidentOnTraceVmId.Should().Be(_trace.TraceId);
            var accidentPlaceNodeVm2 = _sut.GraphReadModel.Data.Nodes
                .First(n=>n.State == FiberState.Major && n.Type == EquipmentType.AccidentPlace);
            accidentPlaceNodeVm2.AccidentOnTraceVmId.Should().Be(_trace.TraceId);

            _sut.ReadModel.Nodes.Count.Should().Be(11);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(2);
            var accidentPlaceNode1 = _sut.ReadModel.Nodes
                .First(n=>n.State == FiberState.Minor && n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace);
            accidentPlaceNode1.AccidentOnTraceId.Should().Be(_trace.TraceId);
            var accidentPlaceNode2 = _sut.ReadModel.Nodes
                .First(n=>n.State == FiberState.Major && n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace);
            accidentPlaceNode2.AccidentOnTraceId.Should().Be(_trace.TraceId);

            _trace.State.Should().Be(FiberState.Major);
            _sut.AssertTraceFibersState(_trace);

            accidentPlaceNodeVm1.Position.ShouldBeEquivalentTo(_crossVm.Position);
            accidentPlaceNodeVm2.Position.ShouldBeEquivalentTo(_closureVm.Position);
            accidentPlaceNode1.Position.ShouldBeEquivalentTo(_crossVm.Position);
            accidentPlaceNode2.Position.ShouldBeEquivalentTo(_closureVm.Position);
        }

        [Then(@"Снова трасса фиолетовая и Кресты совпадающие с проключением и муфтой")]
        public void ThenСноваТрассаФиолетоваяИКрестыСовпадающиеСПроключениемИМуфтой()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(13);
            _sut.GraphReadModel.Data.Nodes.Count(n => n.Type == EquipmentType.AccidentPlace).Should().Be(2);
            var accidentPlaceNodeVm1 = _sut.GraphReadModel.Data.Nodes
                .First(n=>n.State == FiberState.Minor && n.Type == EquipmentType.AccidentPlace);
            accidentPlaceNodeVm1.AccidentOnTraceVmId.Should().Be(_trace.TraceId);
            var accidentPlaceNodeVm2 = _sut.GraphReadModel.Data.Nodes
                .First(n=>n.State == FiberState.Major && n.Type == EquipmentType.AccidentPlace);
            accidentPlaceNodeVm2.AccidentOnTraceVmId.Should().Be(_trace.TraceId);

            _sut.ReadModel.Nodes.Count.Should().Be(13);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(2);
            var accidentPlaceNode1 = _sut.ReadModel.Nodes
                .First(n=>n.State == FiberState.Minor && n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace);
            accidentPlaceNode1.AccidentOnTraceId.Should().Be(_trace.TraceId);
            var accidentPlaceNode2 = _sut.ReadModel.Nodes
                .First(n=>n.State == FiberState.Major && n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace);
            accidentPlaceNode2.AccidentOnTraceId.Should().Be(_trace.TraceId);

            _trace.State.Should().Be(FiberState.Major);
            _sut.AssertTraceFibersState(_trace);

            accidentPlaceNodeVm1.Position.ShouldBeEquivalentTo(_crossVm.Position);
            accidentPlaceNodeVm2.Position.ShouldBeEquivalentTo(_closureVm.Position);
            accidentPlaceNode1.Position.ShouldBeEquivalentTo(_crossVm.Position);
            accidentPlaceNode2.Position.ShouldBeEquivalentTo(_closureVm.Position);
        }


        [Then(@"Отображается что все ОК")]
        public void ThenОтображаетсяЧтоВсеОк()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(9);
            _sut.GraphReadModel.Data.Nodes.Count(n => n.Type == EquipmentType.AccidentPlace).Should().Be(0);

            _sut.ReadModel.Nodes.Count.Should().Be(9);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(0);

            _trace.State.Should().Be(FiberState.Ok);
            _sut.AssertTraceFibersState(_trace);
        }

        [Then(@"Все синее и никаких крестов")]
        public void ThenВсеСинееИНикакихКрестов()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(9);
            _sut.GraphReadModel.Data.Nodes.Count(n => n.Type == EquipmentType.AccidentPlace).Should().Be(0);

            _sut.ReadModel.Nodes.Count.Should().Be(9);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(0);

            _trace.State.Should().Be(FiberState.NotJoined);
            _sut.AssertTraceFibersState(_trace);
        }


    }
}
