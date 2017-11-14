using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceCleanedOrRemovedSteps
    {
        private readonly SutForTraceCleanRemove _sut = new SutForTraceCleanRemove();

        [Given(@"Даны две трассы с общим отрезком")]
        public void GivenДаныДвеТрассыСОбщимОтрезком()
        {
            _sut.CreateTwoTraces();
        }

        [Given(@"Одна из трасс присоединена к порту")]
        public void GivenОднаИзТрассПрисоединенаКПорту()
        {
            var rtuLeaf = _sut.InitializeRtu(_sut.ReadModel.Traces.First(t => t.Id == _sut.TraceId1).RtuId);
            _sut.AttachTraceTo(_sut.TraceId1, rtuLeaf, 2, Answer.Yes);
        }

        [Then(@"У присоединенной трассы нет пунктов Очистить и Удалить в меню")]
        public void ThenУПрисоединеннойТрассыНетПунктовОчиститьИУдалитьВМеню()
        {
            var traceLeaf = _sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_sut.TraceId1);
            traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Clean).Should().BeNull();
            traceLeaf.MyContextMenu.FirstOrDefault(item => item?.Header == Resources.SID_Remove).Should().BeNull();
        }

        [When(@"Пользователь жмет Очистить у НЕприсоединенной трассы")]
        public void WhenПользовательЖметОчиститьУнЕприсоединеннойТрассы()
        {
            var traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_sut.TraceId2);
            _sut.TraceLeafActions.CleanTrace(traceLeaf);
            _sut.Poller.Tick();
        }

        [When(@"Пользователь жмет Удалить у НЕприсоединенной трассы")]
        public void WhenПользовательЖметУдалитьУнЕприсоединеннойТрассы()
        {
            var traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_sut.TraceId2);
            _sut.TraceLeafActions.RemoveTrace(traceLeaf);
            _sut.Poller.Tick();
        }

        [Then(@"Неприсоединенная трасса удаляется")]
        public void ThenНеприсоединеннаяТрассаУдаляется()
        {
            _sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_sut.TraceId2).Should().BeNull();
        }

        [Then(@"Те ее отрезки что не входят в присоединенную трассу меняют цвет")]
        public void ThenТеЕеОтрезкиЧтоНеВходятВПрисоединеннуюТрассуМеняютЦвет()
        {
            foreach (var fiberVm in _sut.ShellVm.GraphReadModel.Fibers)
            {
                if (fiberVm.States.ContainsKey(_sut.TraceId1))
                    fiberVm.State.Should().Be(FiberState.NotChecked);
                else
                    fiberVm.State.Should().Be(FiberState.NotInTrace);
            }
        }

        [Then(@"Те ее отрезки что не входят в присоединенную трассу удаляются")]
        public void ThenТеЕеОтрезкиЧтоНеВходятВПрисоединеннуюТрассуУдаляются()
        {
            _sut.ShellVm.GraphReadModel.Fibers.All(f => f.State == FiberState.NotChecked).Should().BeTrue();
        }
    }
}
