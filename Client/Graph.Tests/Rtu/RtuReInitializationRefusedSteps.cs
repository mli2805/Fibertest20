using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuReInitializationRefusedSteps
    {
        private SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Rtu _rtu;
        private RtuLeaf _rtuLeaf;
        private string _rtuAddress;
        private int _portWithBop;
        private string _rtuSerial;
        private int _ownPortCount;

        [Given(@"Существует RTU с адресом (.*) с длиной волны (.*) и с (.*) портами")]
        public void GivenСуществуетRtuсАдресомСДлинойВолныИсПортами(string p0, string p1, int p2)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            _sut.Poller.EventSourcingTick().Wait();
            _rtu = _sut.ReadModel.Rtus.Last();
            _rtuSerial = @"908070";
            _ownPortCount = p2;
            _sut.FakeD2RWcfManager.SetFakeInitializationAnswer(serial:_rtuSerial, waveLength: p1, ownPortCount: _ownPortCount);
            _rtuAddress = p0;
            _rtuLeaf = _sut.SetNameAndAskInitializationRtu(_rtu.Id, _rtuAddress);
        }

        [Given(@"БОП подключен к порту RTU (.*)")]
        public void GivenБопПодключенКПортуRtu(int p0)
        {
            _portWithBop = p0;
            _sut.AttachOtau(_rtuLeaf, _portWithBop);
        }

        [When(@"RTU заменен на старинный не поддерживающий БОПы RTU")]
        public void WhenRtuЗамененНаСтаринныйНеПоддерживающийБопыRtu()
        {
            _sut.FakeD2RWcfManager.SetFakeInitializationAnswer(ReturnCode.RtuDoesntSupportBop);
        }

        [When(@"RTU заменяется на свежий c (.*) портами")]
        public void WhenRtuЗаменяетсяНаСвежийCПортами(int p0)
        {
            if (_portWithBop > p0)
                _sut.FakeD2RWcfManager.SetFakeInitializationAnswer(ReturnCode.RtuTooBigPortNumber);
            else
                _sut.FakeD2RWcfManager.SetFakeInitializationAnswer(ReturnCode.Ok, ownPortCount: p0);
        }

        [When(@"Пользователь жмет переинициализировать RTU")]
        public void WhenПользовательЖметПереинициализироватьRtu()
        {
            _sut.FakeWindowManager.RegisterHandler(m => m is MyMessageBoxViewModel);
            _sut.FakeWindowManager.RegisterHandler(model =>
                _sut.RtuInitializeHandler(model, _rtuAddress, ""));
            _rtuLeaf.MyContextMenu.First(i => i?.Header == Resources.SID_Network_settings).Command.Execute(_rtuLeaf);
        }

        [Then(@"Выдается сообщение что подключение БОПов не поддерживается")]
        public void ThenВыдаетсяСообщениеЧтоПодключениеБоПовНеПоддерживается()
        {
            _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last()
                .Lines[0].Line
                .Should().Be(Resources.SID_RTU_does_not_support_BOP);
        }

        [Then(@"Выдается сообщение что слишком большой номер порта")]
        public void ThenВыдаетсяСообщениеЧтоСлишкомБольшойНомерПорта()
        {
            _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last()
                .Lines[0].Line
                .Should().Be(Resources.SID_Too_big_port_number_for_BOP_attachment);
        }

        [Then(@"В дереве ничего не меняется")]
        public void ThenВДеревеНичегоНеМеняется()
        {
            _rtuLeaf.Serial.Should().Be(_rtuSerial);
            _rtuLeaf.OwnPortCount.Should().Be(_ownPortCount);
            _rtu.MainChannel.Ip4Address.Should().Be(_rtuAddress);
        }

    }
}
