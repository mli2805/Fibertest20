using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class OtauAttachedSteps
    {
        private SutForTraceAttach _sut = new SutForTraceAttach();
        private Guid _rtuId;
        private Iit.Fibertest.Graph.Trace _trace;
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;
        private int portNumber = 3;

        [Given(@"Существует и инициализирован RTU с неприсоединенной трассой")]
        public void GivenСуществуетИИнициализированRtuсНеприсоединеннойТрассой()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _rtuId = _trace.RtuId;
            _rtuLeaf = _sut.InitializeRtu(_rtuId);
        }

        [Given(@"Трасса подключена к порту")]
        public void GivenТрассаПодключенаКПорту()
        {
            _sut.AttachTrace(_trace.Id, 2, Answer.Yes);
        }

        [Then(@"Пункт подключить переключатель не доступен")]
        public void ThenПунктПодключитьПереключательНеДоступен()
        {
            _rtuLeaf.Children.Any(port => port is PortLeaf && port.MyContextMenu.First(i => i.Header == Resources.SID_Attach_optical_switch)
                .Command.CanExecute(null))
                .Should()
                .BeFalse();
        }

        [Given(@"Пользователь подключает доп переключатель к порту RTU")]
        public void GivenПользовательПодключаетДопПереключательКПортуRtu()
        {
            
            _otauLeaf = _sut.AttachOtauToRtu(_rtuLeaf, portNumber);
        }

        [Then(@"Переключатель подключен")]
        public void ThenПереключательПодключен()
        {
            _rtuLeaf.FullPortCount.Should().Be(24);
            _rtuLeaf.Children[portNumber-1].Should().Be(_otauLeaf);
            _rtuLeaf.GetOwnerOfExtendedPort(9).Should().Be(_otauLeaf);
            _otauLeaf.Children.Count.Should().Be(16);

            _otauLeaf.Children.Any(
                port =>
                    port.MyContextMenu.First(i => i.Header == Resources.SID_Attach_optical_switch)
                        .Command.CanExecute(null)).Should().BeFalse();
        }
    }
}
