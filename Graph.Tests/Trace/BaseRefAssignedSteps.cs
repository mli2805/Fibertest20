using System;
using FluentAssertions;
using Iit.Fibertest.Client;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class BaseRefAssignedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Iit.Fibertest.Graph.Trace _trace;
        private Guid _oldPreciseId;
        private Guid _oldFastId;

        [Given(@"Была создана трасса")]
        public void GivenБылаСозданаТрасса()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
        }

        [When(@"Пользователь указывает пути к точной и быстрой базовам и жмет сохранить")]
        public void WhenПользовательУказываетПутиКТочнойИБыстройБазовамИЖметСохранить()
        {
            var vm = new BaseRefsAssignViewModel(_trace, _sut.ReadModel, _sut.ShellVm.Bus);
            vm.PreciseBaseFilename = SystemUnderTest.Path;
            vm.FastBaseFilename = SystemUnderTest.Path;
            vm.Save();
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
            var vm = new BaseRefsAssignViewModel(_trace, _sut.ReadModel, _sut.ShellVm.Bus);
            vm.FastBaseFilename = SystemUnderTest.Path;
            vm.Save();
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
            var vm = new BaseRefsAssignViewModel(_trace, _sut.ReadModel, _sut.ShellVm.Bus);
            vm.ClearPathToPrecise();
            vm.AdditionalBaseFilename = SystemUnderTest.Path;
            vm.Save();
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
