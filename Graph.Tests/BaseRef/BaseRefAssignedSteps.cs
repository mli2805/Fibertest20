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
        private RtuLeaf _rtuLeaf;
        private TraceLeaf _traceLeaf;
        private Guid _oldPreciseId;
        private Guid _oldFastId;

        [Given(@"Была создана трасса")]
        public void GivenБылаСозданаТрасса()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_trace.Id);
            _rtuLeaf = (RtuLeaf) _traceLeaf.Parent;
        }

        [Then(@"Пункт Задать базовые недоступен")]
        public void ThenПунктЗадатьБазовыеНедоступен()
        {
            _sut.TraceLeafActionsPermissions.CanAssignBaseRefsAction(_traceLeaf).Should().BeFalse();
        }

        [When(@"RTU успешно инициализируется c длинной волны (.*)")]
        public void WhenRtuУспешноИнициализируетсяCДлиннойВолны(string p0)
        {
            _sut.InitializeRtu(_rtuLeaf.Id, p0);
            _rtuLeaf.TreeOfAcceptableMeasParams.Units.ContainsKey(p0).Should().BeTrue();
        }


        [Then(@"Пункт Задать базовые становится доступен")]
        public void ThenПунктЗадатьБазовыеСтановитсяДоступен()
        {
            _sut.TraceLeafActionsPermissions.CanAssignBaseRefsAction(_traceLeaf).Should().BeTrue();
        }


        [When(@"Пользователь указывает пути к точной и быстрой базовам и жмет сохранить")]
        public void WhenПользовательУказываетПутиКТочнойИБыстройБазовамИЖметСохранить()
        {
            _sut.AssignBaseRef(_traceLeaf.Id, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes);
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

            _sut.AssignBaseRef(_traceLeaf.Id, null, SystemUnderTest.Base1625, null, Answer.Yes);
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
            _sut.AssignBaseRef(_traceLeaf.Id, "", null, SystemUnderTest.Base1625, Answer.Yes);
        }

        [Then(@"У трассы не задана точная и старая быстрая и есть дополнительная базовые")]
        public void ThenУТрассыНеЗаданаТочнаяСтараяБыстраяИЕстьДополнительнаяБазовые()
        {
            _trace.PreciseId.Should().Be(Guid.Empty);
            _trace.AdditionalId.Should().NotBe(Guid.Empty);
        }
    }
}
