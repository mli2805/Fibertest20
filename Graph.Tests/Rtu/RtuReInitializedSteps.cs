using System;
using System.Linq;
using Iit.Fibertest.Graph.Requests;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuReInitializedSteps
    {
        private SystemUnderTest _sut = new SystemUnderTest();

        [Given(@"Существует RTU с адресом (.*) с длиной волны (.*) и с (.*) портами")]
        public void GivenСуществуетRtuсАдресомСДлинойВолныИсПортами(string p0, string p1, int p2)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            _sut.Poller.EventSourcingTick().Wait();
            var rtu = _sut.ReadModel.Rtus.Last();
            var rtuLeaf = _sut.InitializeRtu(rtu.Id, p0, "", p1, p2);
        }

        [Given(@"БОП подключен к порту RTU (.*)")]
        public void GivenБопПодключенКПортуRtu(int p0)
        {
        }

        [Given(@"Трасса подключена к порту RTU (.*)")]
        public void GivenТрассаПодключенаКПортуRtu(int p0)
        {
        }

        [Given(@"Трасса подключена к порту БОПа (.*)")]
        public void GivenТрассаПодключенаКПортуБоПа(int p0)
        {
        }

        [When(@"Пользователь жмет переинициализировать RTU")]
        public void WhenПользовательЖметПереинициализироватьRtu()
        {
        }

        [When(@"RTU заменен на старинный не поддерживающий БОПы RTU")]
        public void WhenRtuЗамененНаСтаринныйНеПоддерживающийБопыRtu()
        {
        }

        [Then(@"Выдается сообщение с требование вручную отсоединить БОП и повторить")]
        public void ThenВыдаетсяСообщениеСТребованиеВручнуюОтсоединитьБопиПовторить()
        {
        }

        [Then(@"В дереве ничего не меняется")]
        public void ThenВДеревеНичегоНеМеняется()
        {
        }

    }
}