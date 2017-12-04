using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuInitializedSteps
    {
        private readonly SutForTraceAttach _sut;
        private RtuLeaf _rtuLeaf;
        private string _mainAddress, _reserveAddress;

        public RtuInitializedSteps(SutForTraceAttach sut)
        {
            _sut = sut;
        }

        [Given(@"Существует RTU с основным (.*) и резервным (.*) адресами")]
        public void GivenСуществуетRTUСОсновным_ИРезервным_Адресами(string p0, string p1)
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            _sut.Poller.Tick();
            var rtuId = _sut.ReadModel.Rtus.Last().Id;
            InitializeRtu cmd = new InitializeRtu()
            {
                Id = rtuId,
                MainChannel = new NetAddress(p0, TcpPorts.RtuListenTo),
                IsReserveChannelSet = true,
                ReserveChannel = new NetAddress(p1, TcpPorts.RtuListenTo)
            };
            _sut.ShellVm.C2DWcfManager.SendCommandAsObj(cmd).Wait();
            _sut.Poller.Tick();
        }

        [Given(@"Создан РТУ даже с трассой")]
        public void GivenСозданРтуДажеСТрассой()
        {
            _sut.CreateTraceRtuEmptyTerminal();
            var rtuId = _sut.ReadModel.Rtus.Last().Id;
            _rtuLeaf = (RtuLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(rtuId);
        }

        [When(@"Пользователь вводит основной адрес (.*) и жмет Инициализировать")]
        public void WhenПользовательВводитОсновнойАдрес_ИЖметИнициализировать(string p0)
        {
            _mainAddress = p0;
            _sut.FakeWindowManager.RegisterHandler(m => m is NotificationViewModel);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, _rtuLeaf.Id, p0, "", Answer.Yes));

            _sut.RtuLeafActions.InitializeRtu(_rtuLeaf);
            _sut.Poller.Tick();
        }

        [When(@"Пользователь вводит основной (.*) и резервный (.*) адреса и жмет Инициализировать")]
        public void WhenПользовательВводитОсновной_ИРезервный_АдресаИЖметИнициализировать(string p0, string p1)
        {
            _mainAddress = p0;
            _reserveAddress = p1;
            _sut.FakeWindowManager.RegisterHandler(m => m is NotificationViewModel);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, _rtuLeaf.Id, p0, p1, Answer.Yes));

            _sut.RtuLeafActions.InitializeRtu(_rtuLeaf);
            _sut.Poller.Tick();
        }

        [Then(@"Сообщение об существовании RTU с таким адресом")]
        public void ThenСообщениеОбСуществованииRtuсТакимАдресом()
        {
            var lastNotificationViewModel = _sut.FakeWindowManager.Log
                .OfType<NotificationViewModel>()
                .Last();

            lastNotificationViewModel
               .Message
               .Should().Be(Resources.SID_There_is_RTU_with_the_same_ip_address_);

            _sut.FakeWindowManager.Log.Remove(lastNotificationViewModel);
        }

        [When(@"Пользователь открывает форму инициализации и жмет Отмена")]
        public void WhenПользовательОткрываетФормуИнициализацииИЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, _rtuLeaf.Id, "", "", Answer.Cancel));

            _sut.RtuLeafActions.InitializeRtu(_rtuLeaf);
            _sut.Poller.Tick();
        }

        [Then(@"RTU инициализирован только с основным адресом")]
        public void ThenRtuИнициализированТолькоСОсновнымАдресом()
        {
            _rtuLeaf.ChildrenImpresario.Children.Count.Should().BeGreaterThan(1);
            var rtu = _sut.ReadModel.Rtus.First(r => r.Id == _rtuLeaf.Id);
            rtu.MainChannel.Ip4Address.Should().Be(_mainAddress);
            rtu.IsReserveChannelSet.Should().BeFalse();
        }

        [Then(@"RTU инициализирован с основным и резервным адресами")]
        public void ThenRtuИнициализированСОсновнымИРезервнымАдресами()
        {
            _rtuLeaf.ChildrenImpresario.Children.Count.Should().BeGreaterThan(1);
            var rtu = _sut.ReadModel.Rtus.First(r => r.Id == _rtuLeaf.Id);
            rtu.MainChannel.Ip4Address.Should().Be(_mainAddress);
            rtu.IsReserveChannelSet.Should().BeTrue();
            rtu.ReserveChannel.Ip4Address.Should().Be(_reserveAddress);
        }

        [Then(@"РТУ НЕ инициализирован")]
        public void ThenРтунеИнициализирован()
        {
            _rtuLeaf.ChildrenImpresario.Children.Count.Should().Be(1);
        }

    }
}
