using System;
using FluentAssertions;
using Iit.Fibertest.Client;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class BaseRefAssignedSteps
    {
        private readonly SutForTraceAttach _sut = new SutForTraceAttach();
        private Iit.Fibertest.Graph.Trace _trace;
        private TraceLeaf _traceLeaf;
        private Guid _oldPreciseId;
        private Guid _oldFastId;

        [Given(@"Была создана трасса")]
        public void GivenБылаСозданаТрасса()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_trace.Id);
        }

        [When(@"Пользователь указывает пути к точной и быстрой базовам и жмет сохранить")]
        public void WhenПользовательУказываетПутиКТочнойИБыстройБазовамИЖметСохранить()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, _trace.Id, SystemUnderTest.Path, SystemUnderTest.Path, null, Answer.Yes));
            _traceLeaf.AssignBaseRefsAction(null);
            _sut.Poller.Tick();
        }

        [Then(@"У трассы заданы точная и быстрая базовые")]
        public void ThenУТрассыЗаданыТочнаяИБыстраяБазовые()
        {
            _trace.PreciseId.Should().NotBe(Guid.Empty);
            _trace.FastId.Should().NotBe(Guid.Empty);
        }

        [When(@"Пользователь изменяет быструю и жмет сохранить")]
        public void WhenПользовательИзменяетБыструюИЖметСохранить()
        {
            _oldPreciseId = _trace.PreciseId;
            _oldFastId = _trace.FastId;

            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, _trace.Id, null, SystemUnderTest.Path2, null, Answer.Yes));
            _traceLeaf.AssignBaseRefsAction(null);
            _sut.Poller.Tick();
        }

        [Then(@"У трассы старая точная и новая быстрая базовые")]
        public void ThenУТрассыСтараяТочнаяИНоваяБыстраяБызовые()
        {
            _trace.PreciseId.Should().Be(_oldPreciseId);
            _trace.FastId.Should().NotBe(_oldFastId);
        }

        [When(@"Пользователь сбрасывает точную и задает дополнительную и жмет сохранить")]
        public void WhenПользовательСбрасываетТочнуюЗадаетДополнительнуюИЖметСохранить()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, _trace.Id, "",  null, SystemUnderTest.Path, Answer.Yes));
            _traceLeaf.AssignBaseRefsAction(null);
            _sut.Poller.Tick();
        }

        [Then(@"У трассы не задана точная и старая быстрая и есть дополнительная базовые")]
        public void ThenУТрассыНеЗаданаТочнаяСтараяБыстраяИЕстьДополнительнаяБазовые()
        {
            _trace.PreciseId.Should().Be(Guid.Empty);
            _trace.AdditionalId.Should().NotBe(Guid.Empty);
        }
    }
}
