using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class MoniResultComesSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Trace _trace;
        private TraceLeaf _traceLeaf;

        private NodeVm _crossVm;
        private NodeVm _closureVm;

        [Given(@"Трасса c 4 ориентирами на мониторинге")]
        public void GivenТрассаCОриентирамиНаМониторинге()
        {
            _trace = _sut.SetTraceWithBaseRefs();
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(9);
            _sut.ReadModel.Nodes.Count.Should().Be(9);

            _crossVm = _sut.GraphReadModel.Data.Nodes.First(n => n.Id == _trace.Nodes[1]);
            _closureVm = _sut.GraphReadModel.Data.Nodes.First(n => n.Id == _trace.Nodes[5]);

            _trace.State.Should().Be(FiberState.NotJoined);
            AssertTraceFibersState();

            _traceLeaf = _sut.Attach(_trace, 3);

            _trace.State.Should().Be(FiberState.Unknown);
            AssertTraceFibersState();
        }

        private void AssertTraceFibersState()
        {
            var fibers = _sut.ReadModel.GetTraceFibers(_trace).ToList();
            foreach (var fiber in fibers)
            {
                fiber.States.Contains(new KeyValuePair<Guid, FiberState>(_trace.Id, _trace.State)).Should().Be(true);
                var fiberVm = _sut.GraphReadModel.Data.Fibers.First(f => f.Id == fiber.Id);
                fiberVm.States.Contains(new KeyValuePair<Guid, FiberState>(_trace.Id, _trace.State)).Should().Be(true);
            }
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
                PortWithTrace = new PortWithTraceDto() { TraceId = _trace.Id },
                TraceState = traceState,
                BaseRefType = baseType,
                SorBytes = sorBytes,
            };
            _sut.MsmqHandler.ProcessMonitoringResult(dto).Wait();
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
            accidentPlaceNodeVm.AccidentOnTraceVmId.Should().Be(_trace.Id);

            _sut.ReadModel.Nodes.Count.Should().Be(10);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(1);
            var accidentPlaceNode = _sut.GraphReadModel.Data.Nodes.First(n => n.Type == EquipmentType.AccidentPlace);
            accidentPlaceNode.AccidentOnTraceVmId.Should().Be(_trace.Id);

            _trace.State.Should().Be(FiberState.FiberBreak);
            AssertTraceFibersState();

            accidentPlaceNode.Position.ShouldBeEquivalentTo(_closureVm.Position);
            accidentPlaceNodeVm.Position.ShouldBeEquivalentTo(_closureVm.Position);
        }


        [Then(@"Все фиолетовое и Кресты совпадающие с проключением и муфтой")]
        public void ThenВсеФиолетовоеИКрестыСовпадающиеСПервойИВторойМуфтой()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(11);
            _sut.GraphReadModel.Data.Nodes.Count(n => n.Type == EquipmentType.AccidentPlace).Should().Be(2);
            var accidentPlaceNodeVm1 = _sut.GraphReadModel.Data.Nodes[9];
            accidentPlaceNodeVm1.AccidentOnTraceVmId.Should().Be(_trace.Id);
            var accidentPlaceNodeVm2 = _sut.GraphReadModel.Data.Nodes[10];
            accidentPlaceNodeVm2.AccidentOnTraceVmId.Should().Be(_trace.Id);

            _sut.ReadModel.Nodes.Count.Should().Be(11);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(2);
            var accidentPlaceNode1 = _sut.ReadModel.Nodes[9];
            accidentPlaceNode1.AccidentOnTraceId.Should().Be(_trace.Id);
            var accidentPlaceNode2 = _sut.ReadModel.Nodes[10];
            accidentPlaceNode2.AccidentOnTraceId.Should().Be(_trace.Id);

            _trace.State.Should().Be(FiberState.Major);
            AssertTraceFibersState();

            accidentPlaceNodeVm1.Position.ShouldBeEquivalentTo(_closureVm.Position);
            accidentPlaceNodeVm2.Position.ShouldBeEquivalentTo(_crossVm.Position);
            accidentPlaceNode1.Position.ShouldBeEquivalentTo(_closureVm.Position);
            accidentPlaceNode2.Position.ShouldBeEquivalentTo(_crossVm.Position);
        }

        [Then(@"Снова трасса фиолетовая и Кресты совпадающие с проключением и муфтой")]
        public void ThenСноваТрассаФиолетоваяИКрестыСовпадающиеСПроключениемИМуфтой()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(13);
            _sut.GraphReadModel.Data.Nodes.Count(n => n.Type == EquipmentType.AccidentPlace).Should().Be(2);
            var accidentPlaceNodeVm1 = _sut.GraphReadModel.Data.Nodes[11];
            accidentPlaceNodeVm1.AccidentOnTraceVmId.Should().Be(_trace.Id);
            var accidentPlaceNodeVm2 = _sut.GraphReadModel.Data.Nodes[12];
            accidentPlaceNodeVm2.AccidentOnTraceVmId.Should().Be(_trace.Id);

            _sut.ReadModel.Nodes.Count.Should().Be(13);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(2);
            var accidentPlaceNode1 = _sut.ReadModel.Nodes[11];
            accidentPlaceNode1.AccidentOnTraceId.Should().Be(_trace.Id);
            var accidentPlaceNode2 = _sut.ReadModel.Nodes[12];
            accidentPlaceNode2.AccidentOnTraceId.Should().Be(_trace.Id);

            _trace.State.Should().Be(FiberState.Major);
            AssertTraceFibersState();

            accidentPlaceNodeVm1.Position.ShouldBeEquivalentTo(_closureVm.Position);
            accidentPlaceNodeVm2.Position.ShouldBeEquivalentTo(_crossVm.Position);
            accidentPlaceNode1.Position.ShouldBeEquivalentTo(_closureVm.Position);
            accidentPlaceNode2.Position.ShouldBeEquivalentTo(_crossVm.Position);
        }


        [Then(@"Отображается что все ОК")]
        public void ThenОтображаетсяЧтоВсеОк()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(9);
            _sut.GraphReadModel.Data.Nodes.Count(n => n.Type == EquipmentType.AccidentPlace).Should().Be(0);

            _sut.ReadModel.Nodes.Count.Should().Be(9);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(0);

            _trace.State.Should().Be(FiberState.Ok);
            AssertTraceFibersState();
        }

        [Then(@"Все синее и никаких крестов")]
        public void ThenВсеСинееИНикакихКрестов()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(9);
            _sut.GraphReadModel.Data.Nodes.Count(n => n.Type == EquipmentType.AccidentPlace).Should().Be(0);

            _sut.ReadModel.Nodes.Count.Should().Be(9);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(0);

            _trace.State.Should().Be(FiberState.NotJoined);
            AssertTraceFibersState();
        }


    }
}
