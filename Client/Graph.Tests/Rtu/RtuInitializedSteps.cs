using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuInitializedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private RtuLeaf _rtuLeaf;
        private Iit.Fibertest.Graph.Rtu _rtu;
        private string _mainAddress, _reserveAddress;

        [Given(@"На сервере применена демо лицензия с одним RTU")]
        public void GivenНаСервереПримененаДемоЛицензияСОднимRtu()
        {
            // When DB is initialized the demo license is applied
        }
        
        [When(@"На сервере применена другая лицензия с двумя RTU")]
        public void WhenНаСервереПримененаДругаяЛицензияСДвумяRtu()
        {
            _sut.WcfServiceDesktopC2D.SendCommandAsObj(new ApplyLicense()
            {
                LicenseId = Guid.NewGuid(),
                Owner = @"RtuAtGpsLocationAddedSteps 2 RTU",
                RtuCount = new LicenseParameter(){Value = 2, ValidUntil = DateTime.MaxValue}
            }).Wait();
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Существует RTU с основным (.*) и резервным (.*) адресами")]
        public void GivenСуществуетRTUСОсновным_ИРезервным_Адресами(string p0, string p1)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            _sut.Poller.EventSourcingTick().Wait();

            _sut.SetNameAndAskInitializationRtu(_sut.ReadModel.Rtus.Last().Id, p0, p1);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Given(@"Создан еще РТУ даже с трассой")]
        public void GivenСозданРтуДажеСТрассой()
        {
            _sut.CreateTraceRtuEmptyTerminal();
            _rtu = _sut.ReadModel.Rtus.Last();
            _rtuLeaf = (RtuLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_rtu.Id);
        }

        [When(@"Пользователь вводит основной адрес (.*) и жмет Инициализировать")]
        public void WhenПользовательВводитОсновнойАдрес_ИЖметИнициализировать(string p0)
        {
            _mainAddress = p0;
            _sut.FakeWindowManager.RegisterHandler(m => m is MyMessageBoxViewModel);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, p0, "", Answer.Yes));

            _rtuLeaf.MyContextMenu.FirstOrDefault(i => i.Header == Resources.SID_Network_settings)?.Command.Execute(_rtuLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь вводит основной (.*) и резервный (.*) адреса и жмет Инициализировать")]
        public void WhenПользовательВводитОсновной_ИРезервный_АдресаИЖметИнициализировать(string p0, string p1)
        {
            _mainAddress = p0;
            _reserveAddress = p1;
            _sut.FakeWindowManager.RegisterHandler(m => m is MyMessageBoxViewModel);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, p0, p1, Answer.Yes));

            _rtuLeaf.MyContextMenu.FirstOrDefault(i => i.Header == Resources.SID_Network_settings)?.Command.Execute(_rtuLeaf);
            _sut.Poller.EventSourcingTick().Wait();
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

        [Then(@"Сообщение об существовании RTU с таким адресом")]
        public void ThenСообщениеОбСуществованииRtuсТакимАдресом()
        {
            var lastNotificationViewModel = _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last();

            lastNotificationViewModel
               .Lines[0].Line
               .Should().Be(Resources.SID_There_is_RTU_with_the_same_ip_address_);

            _sut.FakeWindowManager.Log.Remove(lastNotificationViewModel);
        }

       
        [When(@"Пользователь открывает форму инициализации и жмет Отмена")]
        public void WhenПользовательОткрываетФормуИнициализацииИЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, "", "", Answer.Cancel));
            _rtuLeaf.MyContextMenu.First(i => i.Header == Resources.SID_Network_settings).Command.Execute(_rtuLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь задает имя RTU")]
        public void WhenПользовательЗадаетИмяRtu()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            _sut.GraphReadModel.GrmRtuRequests.UpdateRtu(new RequestUpdateRtu() { RtuId = _rtu.Id, NodeId = _rtu.NodeId });
            _sut.Poller.EventSourcingTick().Wait();

        }

        [Then(@"RTU инициализирован только с основным адресом")]
        public void ThenRtuИнициализированТолькоСОсновнымАдресом()
        {
            _rtuLeaf.ChildrenImpresario.Children.Count.Should().BeGreaterThan(1);
            var rtu = _sut.ReadModel.Rtus.First(r => r.Id == _rtuLeaf.Id);
            rtu.MainChannel.Ip4Address.Should().Be(_mainAddress);
            rtu.IsReserveChannelSet.Should().BeFalse();
            rtu.AcceptableMeasParams.Units.Count.Should().NotBe(0);
            _rtuLeaf.TreeOfAcceptableMeasParams.Units.Count.Should().NotBe(0);
        }

        [Then(@"RTU инициализирован с основным и резервным адресами")]
        public void ThenRtuИнициализированСОсновнымИРезервнымАдресами()
        {
            _rtuLeaf.ChildrenImpresario.Children.Count.Should().BeGreaterThan(1);
            var rtu = _sut.ReadModel.Rtus.First(r => r.Id == _rtuLeaf.Id);
            rtu.MainChannel.Ip4Address.Should().Be(_mainAddress);
            rtu.IsReserveChannelSet.Should().BeTrue();
            rtu.ReserveChannel.Ip4Address.Should().Be(_reserveAddress);
            rtu.AcceptableMeasParams.Units.Count.Should().NotBe(0);
            _rtuLeaf.TreeOfAcceptableMeasParams.Units.Count.Should().NotBe(0);
        }

        [Then(@"РТУ НЕ инициализирован")]
        public void ThenРтунеИнициализирован()
        {
            _rtuLeaf.ChildrenImpresario.Children.Count.Should().Be(1);
        }

    }
}
