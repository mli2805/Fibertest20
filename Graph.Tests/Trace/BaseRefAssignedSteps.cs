using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class BaseRefAssignedSteps
    {
        private readonly SystemUnderTest2 _sut;
        private Guid _traceId;
        private Iit.Fibertest.Graph.Trace _trace;
        private Guid _oldPreciseId;
        private Guid _oldFastId;

        public BaseRefAssignedSteps(SystemUnderTest2 sut)
        {
            _sut = sut;
        }

        [When(@"Пользователь указывает пути к точной и быстрой базовам и жмет сохранить")]
        public void WhenПользовательУказываетПутиКТочнойИБыстройБазовамИЖметСохранить()
        {
            _trace = _sut.ReadModel.Traces.Single();
            _traceId = _trace.Id;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, SystemUnderTest2.Path, SystemUnderTest2.Path, null, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new RequestAssignBaseRef() {TraceId = _traceId}).Wait();
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
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, null, SystemUnderTest2.Path, null, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new RequestAssignBaseRef() { TraceId = _traceId }).Wait();
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
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler(model, "", null, SystemUnderTest2.Path, Answer.Yes));
            _sut.ShellVm.ComplyWithRequest(new RequestAssignBaseRef() { TraceId = _traceId }).Wait();
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
