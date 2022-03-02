using FluentAssertions;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceDetalizationSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private Iit.Fibertest.Graph.Trace _trace;

        private TraceDetalization _result;

        [Given(@"Существует трасса с точками привязки")]
        public void GivenСуществуетТрассаСТочкамиПривязки()
        {
            _rtu = _sut.CreateRtuA();
            _trace = _sut.SetTrace(_rtu.NodeId, @"trace1");
        }

        [When(@"Запрошена детализация с уровнем (.*)")]
        public void WhenЗапрошенаДетализацияСУровнем(int p0)
        {
            _result = _sut.ReadModel.GetTraceDetalization(_trace, (EquipmentType) p0);
        }

        [Then(@"Выдает (.*) узлов в трассе")]
        public void ThenВыдаетУзловВТрассе(int p0)
        {
            _result.Nodes.Count.Should().Be(p0);
        }

    }
}
