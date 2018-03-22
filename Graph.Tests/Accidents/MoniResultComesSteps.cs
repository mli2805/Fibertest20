using System.IO;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Dto;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class MoniResultComesSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Trace _trace;

        [Given(@"Трасса c 4 ориентирами на мониторинге")]
        public void GivenТрассаCОриентирамиНаМониторинге()
        {
            _trace = _sut.SetTraceWithBaseRefs();
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(9);
            _sut.ReadModel.Nodes.Count.Should().Be(9);
        }

        [When(@"Приходит авария")]
        public void WhenПриходитАвария()
        {
            var filename = @"BreakBnode2";
            var sorBytes = File.ReadAllBytes($@"..\..\Sut\MoniResults\Trace4Lm\{filename}.sor");
            var dto = new MonitoringResultDto()
            {
                RtuId = _trace.RtuId,
                PortWithTrace = new PortWithTraceDto() { TraceId = _trace.Id},
                TraceState = FiberState.FiberBreak,
                BaseRefType = BaseRefType.Precise,
                SorBytes = sorBytes,
            };
            _sut.MsmqHandler.ProcessMonitoringResult(dto).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Авария отображается")]
        public void ThenАварияОтображается()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(10);
            _sut.GraphReadModel.Data.Nodes.Count(n => n.Type == EquipmentType.AccidentPlace).Should().Be(1);
            var accidentPlaceNodeVm = _sut.GraphReadModel.Data.Nodes.First(n => n.Type == EquipmentType.AccidentPlace);
            accidentPlaceNodeVm.AccidentOnTraceVmId.Should().Be(_trace.Id);

            _sut.ReadModel.Nodes.Count.Should().Be(10);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(1);
            var accidentPlaceNode = _sut.GraphReadModel.Data.Nodes.First(n => n.Type == EquipmentType.AccidentPlace);
            accidentPlaceNode.AccidentOnTraceVmId.Should().Be(_trace.Id);
        }

        [When(@"Приходит OK")]
        public void WhenПриходитOk()
        {
            var filename = @"4lm-Ok";
            var sorBytes = File.ReadAllBytes($@"..\..\Sut\MoniResults\Trace4Lm\{filename}.sor");
            var dto = new MonitoringResultDto()
            {
                RtuId = _trace.RtuId,
                PortWithTrace = new PortWithTraceDto() { TraceId = _trace.Id },
                TraceState = FiberState.Ok,
                BaseRefType = BaseRefType.Precise,
                SorBytes = sorBytes,
            };
            _sut.MsmqHandler.ProcessMonitoringResult(dto).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Отображается что все ОК")]
        public void ThenОтображаетсяЧтоВсеОк()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(9);
            _sut.GraphReadModel.Data.Nodes.Count(n => n.Type == EquipmentType.AccidentPlace).Should().Be(0);

            _sut.ReadModel.Nodes.Count.Should().Be(9);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(0);
        }

    }
}
