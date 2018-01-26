using System;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
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


        [Given(@"Была создана трасса 2 ориентира - 3 узла")]
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
            var errorString = string.Format(Resources.SID_Invalid_parameter___Wave_length__0_, p0);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxContainsStringAnswer(errorString, Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler2(model, SystemUnderTest.Base1625, null, null, Answer.Yes));
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
            var errorString = Resources.SID_There_are_no_thresholds_for_comparison;
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxContainsStringAnswer(errorString, Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler2(model, SystemUnderTest.Base1550Lm2NoThresholds, null, null, Answer.Yes));
            _sut.TraceLeafActions.AssignBaseRefs(_traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Отказ с подсказкой И базовые не заданы")]
        public void ThenОтказСПодсказкойИБазовыеНеЗаданы()
        {
            _trace.PreciseId.Should().Be(Guid.Empty);
            _trace.FastId.Should().Be(Guid.Empty);
        }

        [When(@"Базовые с порогами но колво ориентиров не совпадает ни с узлами ни с оборудованием")]
        public void WhenБазовыеСПорогамиНоКолвоОриентировНеСовпадаетНиСУзламиНиСОборудованием()
        {
            var errorString = string.Format(Resources.SID__0__base_is_not_compatible_with_trace, BaseRefType.Precise.GetLocalizedFemaleString());
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxContainsStringAnswer(errorString, Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler2(model, SystemUnderTest.Base1550Lm4YesThresholds, null, null, Answer.Yes));
            _sut.TraceLeafActions.AssignBaseRefs(_traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Отказ с подсказкой о количестве и того и другого И базовые не заданы")]
        public void ThenОтказСПодсказкойОКоличествеИТогоИДругогоИБазовыеНеЗаданы()
        {
            _trace.PreciseId.Should().Be(Guid.Empty);
            _trace.FastId.Should().Be(Guid.Empty);
        }

        [When(@"И наконец колво ориентиров совпадает с колвом узлов")]
        public void WhenИНаконецКолвоОриентировСовпадаетСКолвомУзлов()
        {
            var message = string.Format(Resources.SID_Trace_length_on_map_is__0__km, 25.63) +
                          Environment.NewLine + string.Format(Resources.SID_Optical_length_is__0__km, 3.27);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxContainsStringAnswer(message, Answer.Yes, model));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.BaseRefAssignHandler2(model, SystemUnderTest.Base1550Lm2YesThresholds, null, null, Answer.Yes));
            _sut.TraceLeafActions.AssignBaseRefs(_traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Выдается инфо по длинам трассы и рефлектограммы И у трассы становится задани базовая")]
        public void ThenВыдаетсяИнфоПоДлинамТрассыИРефлектограммыИуТрассыСтановитсяЗаданиБазовая()
        {
            _trace.PreciseId.Should().NotBe(Guid.Empty);
            _trace.FastId.Should().Be(Guid.Empty);
        }

    }
}
