using System;
using FluentAssertions;
using Iit.Fibertest.Client;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class BaseRefCheckedSteps
    {
        private readonly SutForTraceAttach _sut = new SutForTraceAttach();
        private Iit.Fibertest.Graph.Trace _trace;
        private RtuLeaf _rtuLeaf;
        private TraceLeaf _traceLeaf;


        [Given(@"Была создана трасса 2 ориентира")]
        public void GivenБылаСозданаТрасса()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_trace.Id);
            _rtuLeaf = (RtuLeaf)_traceLeaf.Parent;
        }

        [Then(@"Пункт меню Задать базовые недоступен")]
        public void ThenПунктЗадатьБазовыеНедоступен()
        {
            _sut.TraceLeafActionsPermissions.CanAssignBaseRefsAction(_traceLeaf).Should().BeFalse();
        }

        [When(@"RTU инициализируется c длинной волны (.*)")]
        public void WhenRtuУспешноИнициализируетсяCДлиннойВолны(string p0)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.RtuInitializeHandler(model, _rtuLeaf.Id, @"1.1.1.1", "", p0, Answer.Yes));
            _sut.RtuLeafActions.InitializeRtu(_rtuLeaf);
            _sut.Poller.EventSourcingTick().Wait();
            _rtuLeaf.TreeOfAcceptableMeasParams.Units.ContainsKey(p0).Should().BeTrue();
        }


        [Then(@"Пункт меню Задать базовые становится доступен")]
        public void ThenПунктЗадатьБазовыеСтановитсяДоступен()
        {
            _sut.TraceLeafActionsPermissions.CanAssignBaseRefsAction(_traceLeaf).Should().BeTrue();
        }


        [When(@"Пользователь указывает путь к базовой c длинной волны SM(.*) и жмет сохранить")]
        public void WhenПользовательУказываетПутьКБазовойCДлиннойВолныSmиЖметСохранить(string p0)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
//                        _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxContainsStringAnswer(p0, Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler2(model, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes));
            _sut.TraceLeafActions.AssignBaseRefs(_traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Отказ с указанием неправильной длины волны И базовые не заданы")]
        public void ThenОтказСУказаниемПравильнойДлиныВолныАБазовыеНеЗаданы()
        {
            _trace.PreciseId.Should().Be(Guid.Empty);
            _trace.FastId.Should().Be(Guid.Empty);
        }

        [When(@"Пользователь выбирает базовые с правильной длиной волны но без порогов")]
        public void WhenПользовательВыбираетБазовыеСПравильнойДлинойВолныНоБезПорогов()
        {
        }

        [Then(@"Отказ с подсказкой И базовые не заданы")]
        public void ThenОтказСПодсказкойИБазовыеНеЗаданы()
        {
        }

    }
}
