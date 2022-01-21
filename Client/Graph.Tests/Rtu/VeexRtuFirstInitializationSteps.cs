using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class VeexRtuFirstInitializationSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private RtuLeaf _rtuLeaf;
        private Iit.Fibertest.Graph.Trace _trace;

        [Given(@"На карте есть RTU")]
        public void GivenНаКартеЕстьRtu()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            _sut.Poller.EventSourcingTick().Wait();
            _rtu = _sut.ReadModel.Rtus.Last();
            _rtuLeaf = (RtuLeaf)_sut.TreeOfRtuModel.GetById(_rtu.Id);
        }

        [Given(@"У RTU один поломанный основной переключатель")]
        public void GivenУRTUОдинПоломанныйОсновнойПереключатель()
        {
            _sut.FakeVeexRtuModel.Otaus[0].connected = false;
        }

        [Given(@"У RTU три недоступных основных переключателя")]
        public void GivenУrtuТриНедоступныхОсновныхПереключателя()
        {
            _sut.FakeVeexRtuModel.AddOtau(new NewOtau() { id = "S1_1" });
            _sut.FakeVeexRtuModel.AddOtau(new NewOtau() { id = "S1_2" });
            _sut.FakeVeexRtuModel.Otaus[0].connected = false;
            _sut.FakeVeexRtuModel.Otaus[1].connected = false;
            _sut.FakeVeexRtuModel.Otaus[2].connected = false;
        }

        [Given(@"У RTU нет основного переключателя")]
        public void GivenУrtuНетОсновногоПереключателя()
        {
            var link = $"otaus/{_sut.FakeVeexRtuModel.Otaus[0].id}";
            _sut.FakeVeexRtuModel.DeleteOtau(link);
        }

        [Given(@"У RTU три основных переключателя и только один с восьмью портами доступен")]
        public void GivenУrtuТриОсновныхПереключателяИТолькоОдинСВосьмьюПортамиДоступен()
        {
            _sut.FakeVeexRtuModel.Otaus[0].connected = false;

            _sut.FakeVeexRtuModel.AddOtau(new NewOtau() { id = "S1_1" });
            _sut.FakeVeexRtuModel.Otaus[1].portCount = 8;
            _sut.FakeVeexRtuModel.Otaus[1].connected = true;

            _sut.FakeVeexRtuModel.AddOtau(new NewOtau() { id = "S1_2" });
            _sut.FakeVeexRtuModel.Otaus[2].connected = false;
        }

        [When(@"Первая инициализация этого RTU")]
        public void WhenПерваяИнициализацияЭтогоRtu()
        {
            _sut.SetNameAndAskInitializationRtu(_rtu.Id, "11.11.11.11", null, 80);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"В дереве у RTU портов - (.*)")]
        public void ThenВДеревеУRTUПортов_(int p0)
        {
            _rtuLeaf.ChildrenImpresario.Children.Count.ShouldBeEquivalentTo(p0);
        }

    }
}
