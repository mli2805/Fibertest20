using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class BaseRefCheckedSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Iit.Fibertest.Graph.Trace _trace;
        private RtuLeaf _rtuLeaf;
        private TraceLeaf _traceLeaf;


        [Given(@"Была создана трасса 2 ориентира - 3 узла")]
        public void GivenБылаСозданаТрасса()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            _rtuLeaf = (RtuLeaf)_traceLeaf.Parent;
        }

        [Then(@"Пункт меню Задать базовые недоступен")]
        public void ThenПунктЗадатьБазовыеНедоступен()
        {
            _traceLeaf.MyContextMenu.First(i => i?.Header == Resources.SID_Base_refs_assignment).Command.CanExecute(_traceLeaf).Should().BeFalse();
        }

        [When(@"RTU инициализируется c длинной волны (.*)")]
        public void WhenRtuУспешноИнициализируетсяCДлиннойВолны(string p0)
        {
            _sut.FakeD2RWcfManager.SetFakeInitializationAnswer(waveLength:p0);
            _sut.SetNameAndAskInitializationRtu(_rtuLeaf.Id);
        }


        [Then(@"Пункт меню Задать базовые становится доступен")]
        public void ThenПунктЗадатьБазовыеСтановитсяДоступен()
        {
            _traceLeaf.MyContextMenu.First(i => i?.Header == Resources.SID_Base_refs_assignment).Command.CanExecute(_traceLeaf).Should().BeTrue();
        }


        [When(@"Пользователь указывает путь к базовым c длинной волны SM(.*) и жмет сохранить")]
        public void WhenПользовательУказываетПутьКБазовымCДлиннойВолныSmиЖметСохранить(string p0)
        {
            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1625, SystemUnderTest.Base1625, null, Answer.Yes);
        }

        [Then(@"Отказ с указанием неправильной длины волны SM(.*) И базовые не заданы")]
        public void ThenОтказСУказаниемПравильнойДлиныВолныАБазовыеНеЗаданы(string p0)
        {
            var lastNotificationViewModel = _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last();

            lastNotificationViewModel.Lines.FirstOrDefault(
                l => l.Line == string.Format(Resources.SID_Invalid_parameter___Wave_length__0_, p0)).Should().NotBe(null);

            _sut.FakeWindowManager.Log.Remove(lastNotificationViewModel);

            _trace.PreciseId.Should().Be(Guid.Empty);
            _trace.FastId.Should().Be(Guid.Empty);
        }

        [When(@"Пользователь выбирает базовые с правильной длиной волны но без порогов")]
        public void WhenПользовательВыбираетБазовыеСПравильнойДлинойВолныНоБезПорогов()
        {
            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1550Lm2NoThresholds, SystemUnderTest.Base1550Lm2NoThresholds, null, Answer.Yes);
        }

        [Then(@"Отказ с подсказкой И базовые не заданы")]
        public void ThenОтказСПодсказкойИБазовыеНеЗаданы()
        {
            var lastNotificationViewModel = _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last();

            lastNotificationViewModel.Lines.FirstOrDefault(
                l => l.Line == Resources.SID_There_are_no_thresholds_for_comparison).Should().NotBe(null);

            _sut.FakeWindowManager.Log.Remove(lastNotificationViewModel);

            _trace.PreciseId.Should().Be(Guid.Empty);
            _trace.FastId.Should().Be(Guid.Empty);
        }

        [When(@"Базовые с порогами но колво ориентиров не совпадает ни с узлами ни с оборудованием")]
        public void WhenБазовыеСПорогамиНоКолвоОриентировНеСовпадаетНиСУзламиНиСОборудованием()
        {
            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1550Lm4YesThresholds, SystemUnderTest.Base1550Lm4YesThresholds, null, Answer.Yes);
        }

        [Then(@"Отказ с подсказкой о количестве и того и другого И базовые не заданы")]
        public void ThenОтказСПодсказкойОКоличествеИТогоИДругогоИБазовыеНеЗаданы()
        {
            var lastNotificationViewModel = _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last();

            var errorString = string.Format(Resources.SID__0__base_is_not_compatible_with_trace, BaseRefType.Precise.GetLocalizedFemaleString());
            lastNotificationViewModel.Lines.FirstOrDefault(l => l.Line == errorString).Should().NotBe(null);

            _sut.FakeWindowManager.Log.Remove(lastNotificationViewModel);

            _trace.PreciseId.Should().Be(Guid.Empty);
            _trace.FastId.Should().Be(Guid.Empty);
        }

        [When(@"И наконец колво ориентиров совпадает с колвом узлов")]
        public void WhenИНаконецКолвоОриентировСовпадаетСКолвомУзлов()
        {
            _sut.AssignBaseRef(_traceLeaf, SystemUnderTest.Base1550Lm2YesThresholds, SystemUnderTest.Base1550Lm2YesThresholds, null, Answer.Yes);
        }

        [Then(@"Выдается инфо по длинам трассы и рефлектограммы И у трассы становится задани базовая")]
        public void ThenВыдаетсяИнфоПоДлинамТрассыИРефлектограммыИуТрассыСтановитсяЗаданиБазовая()
        {
            var lastNotificationViewModel = _sut.FakeWindowManager.Log
                .OfType<MyMessageBoxViewModel>()
                .Last();

            var message = string.Format(Resources.SID_Trace_length_on_map_is__0__km, 25.63) +
                          Environment.NewLine + string.Format(Resources.SID_Optical_length_is__0__km, 3.27);
            lastNotificationViewModel.Lines.FirstOrDefault(l => l.Line == message).Should().NotBe(null);

            _sut.FakeWindowManager.Log.Remove(lastNotificationViewModel);

            _trace.PreciseId.Should().NotBe(Guid.Empty);
            _trace.FastId.Should().NotBe(Guid.Empty);
        }

    }
}
