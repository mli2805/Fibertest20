using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class RtuInitializedSteps
    {
        private readonly SutForTraceAttach _sut = new SutForTraceAttach();
        private RtuLeaf _rtuLeaf;

        [Given(@"Существует RTU с основным (.*) и резервным (.*) адресами")]
        public void GivenСуществуетRTUСОсновным_ИРезервным_Адресами(string p0, string p1)
        {
            _sut.ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            _sut.Poller.Tick();
            var rtuId = _sut.ReadModel.Rtus.Last().Id;
            InitializeRtu cmd = new InitializeRtu()
            {
                Id = rtuId,
                MainChannel = new NetAddress(p0, 11832),
                ReserveChannel = new NetAddress(p1, 11832)
            };
            _sut.ShellVm.Bus.SendCommand(cmd);
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
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, _rtuLeaf.Id, p0, Answer.Yes));

            _rtuLeaf.RtuSettingsAction(null);
        }

        [Then(@"Сообщение об существовании RTU с таким адресом")]
        public void ThenСообщениеОбСуществованииRtuсТакимАдресом()
        {
            _sut.FakeWindowManager.Log
               .OfType<NotificationViewModel>()
               .Last()
               .Message
               .Should().Be(Resources.SID_There_is_RTU_with_the_same_ip_address_);
        }

        [When(@"Пользователь открывает форму инициализации и жмет Инициализировать")]
        public void WhenПользовательОткрываетФормуИнициализацииИЖметИнициализировать()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, _rtuLeaf.Id, "", Answer.Yes));

            _rtuLeaf.RtuSettingsAction(null);
            _sut.Poller.Tick();
        }

        [When(@"Пользователь открывает форму инициализации и жмет Отмена")]
        public void WhenПользовательОткрываетФормуИнициализацииИЖметОтмена()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, _rtuLeaf.Id, "", Answer.Cancel));

            _rtuLeaf.RtuSettingsAction(null);
            _sut.Poller.Tick();
        }

        [Then(@"РТУ инициализирован")]
        public void ThenРтуИнициализирован()
        {
            _rtuLeaf.ChildrenImpresario.Children.Count.Should().BeGreaterThan(1);
        }

        [Then(@"РТУ НЕ инициализирован")]
        public void ThenРтунеИнициализирован()
        {
            _rtuLeaf.ChildrenImpresario.Children.Count.Should().Be(1);
        }

    }
}
