using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceCleanedSteps
    {
        SutForTraceCleanRemove _sut = new SutForTraceCleanRemove();
        private Guid _traceId1, _traceId2;

        [Given(@"Даны две трассы с общим отрезком")]
        public void GivenДаныДвеТрассыСОбщимОтрезком()
        {
            _sut.CreateTwoTraces(out _traceId1, out _traceId2);
        }

        [Given(@"Одна из трасс присоединена к порту")]
        public void GivenОднаИзТрассПрисоединенаКПорту()
        {
            _sut.AttachTrace(_traceId1);
        }

        [Then(@"У присоединенной трассы нет пункта Очистить в меню")]
        public void ThenУПрисоединеннойТрассыНетПунктаОчиститьВМеню()
        {
            var traceLeaf = _sut.ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(_traceId1);
            traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Clean).Should().BeNull();
        }

        [When(@"Пользователь жмет Очистить у НЕприсоединенной трассы")]
        public void WhenПользовательЖметОчиститьУнЕприсоединеннойТрассы()
        {
            var traceLeaf = (TraceLeaf)_sut.ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(_traceId2);
            traceLeaf.TraceCleanAction(null);
            _sut.Poller.Tick();
        }

        [Then(@"Неприсоединенная трасса удаляется")]
        public void ThenНеприсоединеннаяТрассаУдаляется()
        {
            _sut.ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(_traceId2).Should().BeNull();
        }

        [Then(@"Те ее отрезки что не входят в присоединенную трассу меняют цвет")]
        public void ThenТеЕеОтрезкиЧтоНеВходятВПрисоединеннуюТрассуМеняютЦвет()
        {
            foreach (var fiberVm in _sut.ShellVm.GraphReadModel.Fibers)
            {
                if (fiberVm.States.ContainsKey(_traceId1))
                    fiberVm.State.Should().Be(FiberState.NotChecked);
                else
                    fiberVm.State.Should().Be(FiberState.NotInTrace);
            }
        }

    }
}
