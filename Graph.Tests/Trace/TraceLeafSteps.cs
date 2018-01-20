using System;
using System.Windows;
using FluentAssertions;
using Iit.Fibertest.Client;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceLeafSteps
    {
        private readonly SutForTraceAttach _sut = new SutForTraceAttach();
        private Guid _traceId;
        private TraceLeaf _traceLeaf;
        private int _portNumber;
        private RtuLeaf _rtuLeaf;


        [Given(@"У инициализированного RTU cоздаем трассу с названием (.*)")]
        public void GivenСоздаемТрассуСНазваниемТрасса(string p0)
        {
            _rtuLeaf = _sut.TraceCreatedAndRtuInitialized(out _traceId, out var _, p0);
        }

        [Then(@"В дереве появляется лист с названием (.*) без пиктограмм")]
        public void ThenВДеревеПоявляетсяЛистСНазваниемТрасса(string p0)
        {
            _traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_traceId);
            _traceLeaf.Name.ShouldBeEquivalentTo(p0);
            _traceLeaf.PortNumber.ShouldBeEquivalentTo(0);
            _traceLeaf.BaseRefsSet.HasEnoughBaseRefsToPerformMonitoring.Should().BeFalse();
            _traceLeaf.IconsVisibility.Should().Be(Visibility.Hidden);
            _traceLeaf.BaseRefsSet.MonitoringPictogram.ShouldBeEquivalentTo(@"pack://application:,,,/Resources/LeftPanel/EmptySquare.png");
        }

        [When(@"Задаем точную базовую")]
        public void WhenЗадаемТочнуюБазовую()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, _traceId, SystemUnderTest.Path, null, null, Answer.Yes));
            _sut.TraceLeafActions.AssignBaseRefs(_traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Лист трассы получает ее идентификатор остальное не меняется")]
        public void ThenЛистТрассыПолучаетЕеИдентификаторОстальноеНеМеняется()
        {
            _traceLeaf.BaseRefsSet.PreciseId.Should().NotBe(Guid.Empty);
            _traceLeaf.BaseRefsSet.FastId.Should().Be(Guid.Empty);
            _traceLeaf.BaseRefsSet.HasEnoughBaseRefsToPerformMonitoring.Should().BeFalse();
        }

        [When(@"Задаем быструю базовую")]
        public void WhenЗадаемБыструюБазовую()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, _traceId, null, SystemUnderTest.Path, null, Answer.Yes));
            _sut.TraceLeafActions.AssignBaseRefs(_traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Лист трассы получает идентификатор быстрой остальное не меняется")]
        public void ThenЛистТрассыПолучаетИдентификаторБыстройОстальноеНеМеняется()
        {
            _traceLeaf.BaseRefsSet.PreciseId.Should().NotBe(Guid.Empty);
            _traceLeaf.BaseRefsSet.FastId.Should().NotBe(Guid.Empty);
            _traceLeaf.BaseRefsSet.HasEnoughBaseRefsToPerformMonitoring.Should().BeTrue();
        }

        [When(@"Присоединяем трассу к порту (.*)")]
        public void WhenПрисоединяемТрассуКПорту(int p0)
        {
            _portNumber = p0;
            _sut.AttachTraceTo(_traceId, _rtuLeaf, _portNumber, Answer.Yes);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Новый лист трассы на месте листа порта получает имя (.*) и видимые пиктограммы")]
        public void ThenНовыйЛистТрассыНаМестеЛистаПортаПолучаетИмяNТрассаИВидимыеПиктограммы(string p0)
        {
            _traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_traceId);
            _traceLeaf.Name.ShouldBeEquivalentTo(p0);
            _traceLeaf.PortNumber.ShouldBeEquivalentTo(_portNumber);
            _traceLeaf.BaseRefsSet.HasEnoughBaseRefsToPerformMonitoring.Should().BeTrue();
            _traceLeaf.IconsVisibility.Should().Be(Visibility.Visible);
            _traceLeaf.TraceStatePictogram.ShouldBeEquivalentTo(@"pack://application:,,,/Resources/LeftPanel/EmptySquare.png");
            _traceLeaf.BaseRefsSet.MonitoringPictogram.ShouldBeEquivalentTo(@"pack://application:,,,/Resources/LeftPanel/GreyHalfSquare.png");
        }

        [When(@"Удаляем быструю базовую")]
        public void WhenУдаляемБыструюБазовую()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, _traceId, null, "", null, Answer.Yes));
            _sut.TraceLeafActions.AssignBaseRefs(_traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Первая пиктограмма изменяется")]
        public void ThenПерваяПиктограммаИзменяется()
        {
            _traceLeaf.BaseRefsSet.MonitoringPictogram.ShouldBeEquivalentTo(@"pack://application:,,,/Resources/LeftPanel/EmptySquare.png");
        }

    }
}
