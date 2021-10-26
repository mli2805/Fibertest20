using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Dto;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class MoniResultAccidentBetweenNodesSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginAsRoot();
        private Iit.Fibertest.Graph.Trace _trace;

        [Given(@"Трасса с 5 ориентирами на мониторинге")]
        public void GivenТрассаСОриентирамиНаМониторинге()
        {
            _trace = _sut.SetTraceWithAccidentBetweenNodes();
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(5);
            _sut.ReadModel.Nodes.Count.Should().Be(5);

            _trace.State.Should().Be(FiberState.NotJoined);
            _sut.AssertTraceFibersState(_trace);

            _sut.Attach(_trace, 3);

            _trace.State.Should().Be(FiberState.Unknown);
            _sut.AssertTraceFibersState(_trace);
        }

        [When(@"Приходи результат (.*) (.*) с файлом (.*)")]
        public void WhenПриходиРезультатFastFiberBreakСФайломTracelmBreakBnode(string basetype, string seriousness, string filename)
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

        [Then(@"Все красное и Крест между муфтой и проключением")]
        public void ThenВсеКрасноеИКрестМеждуМуфтойИПроключением()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(6);
            _sut.GraphReadModel.Data.Nodes.Count(n => n.Type == EquipmentType.AccidentPlace).Should().Be(1);
            var accidentPlaceNodeVm = _sut.GraphReadModel.Data.Nodes.First(n => n.Type == EquipmentType.AccidentPlace);
            accidentPlaceNodeVm.AccidentOnTraceVmId.Should().Be(_trace.TraceId);

            _sut.ReadModel.Nodes.Count.Should().Be(6);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(1);
            var accidentPlaceNode = _sut.GraphReadModel.Data.Nodes.First(n => n.Type == EquipmentType.AccidentPlace);
            accidentPlaceNode.AccidentOnTraceVmId.Should().Be(_trace.TraceId);

            _trace.State.Should().Be(FiberState.FiberBreak);
            _sut.AssertTraceFibersState(_trace);

//            accidentPlaceNode.Position.ShouldBeEquivalentTo(_closureVm.Position);
//            accidentPlaceNodeVm.Position.ShouldBeEquivalentTo(_closureVm.Position);
        }

    }
}
