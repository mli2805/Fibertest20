using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.StringResources;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceWithLoopCleanOrRemoveSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest().LoginOnEmptyBaseAsRoot();
        private Iit.Fibertest.Graph.Trace _trace;

        [Given(@"Существует трасса с петлей")]
        public void GivenСуществуетТрассаСПетлей()
        {
            _trace = _sut.CreateTraceDoublePassingClosure();
        }

        [When(@"Пользователь очищает трассу")]
        public void WhenПользовательОчищаетТрассу()
        {
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            traceLeaf.MyContextMenu.First(item => item?.Header == Resources.SID_Clean).Command.Execute(traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [When(@"Пользователь удаляет трассу")]
        public void WhenПользовательУдаляетТрассу()
        {
            var traceLeaf = (TraceLeaf)_sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId);
            _sut.FakeWindowManager.RegisterHandler(model => _sut.ManyLinesMessageBoxAnswer(Answer.Yes, model));
            traceLeaf.MyContextMenu.First(item => item?.Header == Resources.SID_Remove).Command.Execute(traceLeaf);
            _sut.Poller.EventSourcingTick().Wait();
        }

        [Then(@"Трасса удаляется")]
        public void ThenТрассаУдаляется()
        {
            _sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(_trace.TraceId).Should().BeNull();
        }

    }
}