using Iit.Fibertest.Client;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceLeafSteps
    {
        private readonly SutForTraceAttach _sut = new SutForTraceAttach();
        private Iit.Fibertest.Graph.Trace _trace;
        private TraceLeaf _traceLeaf;

        [Given(@"Создаем трассу с названием (.*)")]
        public void GivenСоздаемТрассуСНазваниемТрасса(string p0)
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();

        }

        [Then(@"В дереве появляется лист с названием (.*)")]
        public void ThenВДеревеПоявляетсяЛистСНазваниемТрасса(string p0)
        {
            _traceLeaf = (TraceLeaf)_sut.ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(_trace.Id);
        }

    }
}
