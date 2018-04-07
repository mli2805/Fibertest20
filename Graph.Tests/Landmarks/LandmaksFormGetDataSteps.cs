using Autofac;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class LandmaksFormGetDataSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Trace _trace;
        private LandmarksViewModel _vm;

        [Given(@"Задана трасса с поинтами")]
        public void GivenЗаданаТрассаСПоинтами()
        {
            _trace = _sut.SetTraceForLandmarks();
        }

        [When(@"Пользователь открывает форму ориентиров")]
        public void WhenПользовательОткрываетФормуОриентиров()
        {
            _vm = _sut.ClientContainer.Resolve<LandmarksViewModel>();
            _vm.Initialize(_trace.TraceId, false).Wait();
        }

        [Then(@"Проверяем вьюмодель")]
        public void ThenПроверяемВьюмодель()
        {
            _vm.Rows.Count.Should().Be(7);
            _vm.Rows[0].EquipmentType.Should().Be(@"RTU");
            _vm.Rows[1].EquipmentType.Should().Be(Resources.SID_Closure);
            _vm.Rows[2].EquipmentType.Should().Be(Resources.SID_Other);
            _vm.Rows[3].EquipmentType.Should().Be(Resources.SID_Cross);
            _vm.Rows[4].EquipmentType.Should().Be(Resources.SID_CableReserve);
            _vm.Rows[5].EquipmentType.Should().Be(Resources.SID_Node_without_equipment);
            _vm.Rows[6].EquipmentType.Should().Be(Resources.SID_Terminal);
        }

    }
}
