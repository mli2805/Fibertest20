using System.IO;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Dto;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class MoniResultLossCoefSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Trace _trace;

        [Given(@"Трасса с 6 ориентирами на мониторинге")]
        public void GivenТрассаСОриентирамиНаМониторинге()
        {
            _trace = _sut.SetTraceWithLossCoef();
        }

        [When(@"Приходит резалт с файлом (.*)")]
        public void WhenПриходитРезалтСФайломCriticalRnode_MinorLnode_MinorCnode(string filename)
        {
            var sorBytes = File.ReadAllBytes($@"..\..\Sut\MoniResults\{filename}.sor");
            var dto = new MonitoringResultDto()
            {
                RtuId = _trace.RtuId,
                PortWithTrace = new PortWithTraceDto() { TraceId = _trace.TraceId, OtauPort = new OtauPortDto()},
                TraceState = FiberState.Critical,
                BaseRefType = BaseRefType.Precise,
                SorBytes = sorBytes,
            };
            _sut.MsmqMessagesProcessor.ProcessMonitoringResult(dto).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Все красное а отрезок с превышением затухания сиреневый и его правый узел с сиреневым крестом")]
        public void ThenВсеКрасноеАОтрезокСПревышениемЗатуханияСиреневыйИЕгоПравыйУзелССиреневымКрестом()
        {
            _sut.GraphReadModel.Data.Nodes.Count.Should().Be(8);
            _sut.GraphReadModel.Data.Nodes.Count(n => n.Type == EquipmentType.AccidentPlace).Should().Be(2);

            var accidentPlaceNodeVm5 = _sut.GraphReadModel.Data.Nodes.First(n => n.Type == EquipmentType.AccidentPlace && n.State == FiberState.Critical);
            accidentPlaceNodeVm5.AccidentOnTraceVmId.Should().Be(_trace.TraceId);
            var nodeVm5 = _sut.GraphReadModel.Data.Nodes.First(n => n.Id == _trace.NodeIds[5]);
            accidentPlaceNodeVm5.Position.ShouldBeEquivalentTo(nodeVm5.Position);

            var accidentPlaceNodeVm3 = _sut.GraphReadModel.Data.Nodes.First(n => n.Type == EquipmentType.AccidentPlace && n.State == FiberState.Minor);
            accidentPlaceNodeVm3.AccidentOnTraceVmId.Should().Be(_trace.TraceId);
            var nodeVm3 = _sut.GraphReadModel.Data.Nodes.First(n => n.Id == _trace.NodeIds[3]);
            accidentPlaceNodeVm3.Position.ShouldBeEquivalentTo(nodeVm3.Position);


            _sut.ReadModel.Nodes.Count.Should().Be(8);
            _sut.ReadModel.Nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace).Should().Be(2);

            _trace.State.Should().Be(FiberState.Critical);
            _sut.AssertTraceFibersState(_trace);

            var fibers = _trace.FiberIds.ToList();
            _sut.GraphReadModel.Data.Fibers.First(f => f.Id == fibers[2]).State.Should().Be(FiberState.Minor);
        }

    }
}