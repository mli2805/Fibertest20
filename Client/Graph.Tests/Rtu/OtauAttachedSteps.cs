using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class OtauAttachedSteps
    {
        private SystemUnderTest _sut = new SystemUnderTest();
        private Guid _rtuId;
        private Iit.Fibertest.Graph.Trace _trace;
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;
        private int _portNumberForTrace;
        private int _portNumberForOtau;

        [Given(@"Существует и инициализирован RTU с неприсоединенной трассой")]
        public void GivenСуществуетИИнициализированRtuсНеприсоединеннойТрассой()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _rtuId = _trace.RtuId;
            _rtuLeaf = _sut.SetNameAndAskInitializationRtu(_rtuId);
        }

        [Given(@"Трасса подключена к порту (.*)")]
        public void GivenТрассаПодключенаКПорту(int p0)
        {
            _portNumberForTrace = p0;
            var traceLeaf = _sut.AttachTraceTo(_trace.TraceId, _rtuLeaf, _portNumberForTrace, Answer.Yes);
            traceLeaf.PortNumber.Should().Be(_portNumberForTrace);
        }

        [Then(@"Пункт подключить переключатель доступен для остальных портов")]
        public void ThenПунктПодключитьПереключательДоступенДляОстальныхПортов()
        {
            var freePorts = _rtuLeaf.ChildrenImpresario.Children.Where(port => port is PortLeaf);
            freePorts.All(port => port is PortLeaf portLeaf && port.MyContextMenu.First(i => i.Header == Resources.SID_Attach_optical_switch)
                .Command.CanExecute(portLeaf)).Should().BeTrue();
        }

        [Given(@"Пользователь подключает доп переключатель (.*) (.*) к порту RTU (.*)")]
        public void GivenПользовательПодключаетДопПереключатель_КПортуRTU(string p0, int p1, int p2)
        {
            _portNumberForOtau = p2;
            _otauLeaf = _sut.AttachOtau(_rtuLeaf, p2, p0, p1);
        }

        [Then(@"Повторно к другому порту с таким же адресом не получится")]
        public void ThenПовторноКДругомуПортуСТакимЖеАдресомНеПолучится()
        {
            _sut.FakeWindowManager.RegisterHandler(m => m is MyMessageBoxViewModel);
        }

        [Then(@"Переключатель подключен")]
        public void ThenПереключательПодключен()
        {
            _sut.ReadModel.Otaus.FirstOrDefault(o => o.Id == _otauLeaf.Id).Should().NotBeNull();

            _rtuLeaf.FullPortCount.Should().Be(23);
            _rtuLeaf.ChildrenImpresario.Children[_portNumberForOtau-1].Should().Be(_otauLeaf);
            _otauLeaf.ChildrenImpresario.Children.Count.Should().Be(16);

            _otauLeaf.ChildrenImpresario.Children.Any(
                port => port.MyContextMenu.First(i => i.Header == Resources.SID_Attach_optical_switch)
                            .Command.CanExecute(port)).Should().BeFalse();
        }

        [Then(@"Выдается сообщение что такой адрес уже существует")]
        public void ThenВыдаетсяСообщениеЧтоТакойАдресУжеСуществует()
        {
            _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last()
                .Lines[0].Line
                .Should().Be(Resources.SID_There_is_optical_switch_with_the_same_tcp_address_);
        }

        [Then(@"Подключено два переключателя")]
        public void ThenПодключеноДваПереключателя()
        {
            _rtuLeaf.FullPortCount.Should().Be(38);
        }


    }
}
