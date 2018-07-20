using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuAtGpsLocationAddedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private int _rtuCutOff;

        [Given(@"На сервере применена демо лицензия с одним RTU")]
        public void GivenНаСервереПримененаДемоЛицензияСОднимRtu()
        {
            _sut.WcfServiceForClient.SendCommandAsObj(new ApplyLicense() { RtuCount = 1}).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"На сервере применена другая лицензия с двумя RTU")]
        public void WhenНаСервереПримененаДругаяЛицензияСДвумяRtu()
        {
            _sut.WcfServiceForClient.SendCommandAsObj(new ApplyLicense() { RtuCount = 2 }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }


        [When(@"Пользователь кликает добавить РТУ")]
        public void WhenUserClicksAddRtu()
        {
            _sut.Poller.EventSourcingTick().Wait();
            _rtuCutOff = _sut.ReadModel.Rtus.Count;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            _sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation());
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Новый РТУ сохраняется")]
        public void ThenNewRtuPersisted()
        {
            _sut.ReadModel.Rtus.Count.Should().Be(_rtuCutOff+1);
            var rtu = _sut.ReadModel.Rtus.Last();
            var rtuLeaf = (RtuLeaf)_sut.TreeOfRtuModel.GetById(rtu.Id);
            rtuLeaf.MainChannelState.Should().Be(RtuPartState.NotSetYet);
            rtuLeaf.ReserveChannelState.Should().Be(RtuPartState.NotSetYet);
            rtuLeaf.TreeOfAcceptableMeasParams.Should().BeNull();
        }

        [Then(@"Выдается сообщение о превышеном лимите")]
        public void ThenВыдаетсяСообщениеОПревышеномЛимите()
        {
            _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last()
                .Lines[0].Line
                .Should().Be(Resources.SID_Exceeded_the_number_of_RTU_for_an_existing_license);
        }

    }
}
