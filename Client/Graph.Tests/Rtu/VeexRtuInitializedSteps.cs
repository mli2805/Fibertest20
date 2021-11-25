using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class VeexRtuInitializedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Iit.Fibertest.Graph.Rtu _rtu;

        [Given(@"На карту добавлен RTU")]
        public void GivenНаКартуДобавленRtu()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            _sut.Poller.EventSourcingTick().Wait();
            _rtu = _sut.ReadModel.Rtus.Last();
        }

        [When(@"Инициализируем этот RTU")]
        public void WhenИнициализируемЭтотRtu()
        {
            _sut.SetNameAndAskInitializationRtu(_rtu.Id, "11.11.11.11", null, 80);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"В списке переключателей появляется главный переключатель этого RTU")]
        public void ThenВСпискеПереключателейПоявляетсяГлавныйПереключательЭтогоRtu()
        {
            _sut.ReadModel.Otaus.FirstOrDefault(o => o.RtuId == _rtu.Id && o.VeexRtuMainOtauId == _rtu.MainVeexOtau.id)
                .Should().NotBe(null);
        }

    }
}
