using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SutForTraceAttach : SystemUnderTest
    {
        public void InitializeRtu(Guid rtuId, int portCount)
        {
            ShellVm.ComplyWithRequest(new InitializeRtu()
            {
                Id = rtuId,
                FullPortCount = portCount,
                OwnPortCount = portCount,
                MainChannel = new NetAddress(),
                ReserveChannel = new NetAddress(),
            }).Wait();
            Poller.Tick();
        }
        public void AttachTrace(Guid traceId, int port, Answer answer)
        {
            var traceLeaf = ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(traceId);
            FakeWindowManager.RegisterHandler(model => TraceToAttachHandler(model, answer));
            var rtuLeaf = traceLeaf.Parent;
            var portLeaf = (PortLeaf)rtuLeaf.Children[port - 1];
            portLeaf.AttachFromListAction(null);
            Poller.Tick();
        }

        public void TraceCreatedAndRtuInitialized(out Guid traceId, out Guid rtuId)
        {
            traceId = CreateTraceRtuEmptyTerminal().Id;
            var id = traceId;
            rtuId = ReadModel.Traces.First(t => t.Id == id).RtuId;
            InitializeRtu(rtuId, 8);
        }

        public bool TraceToAttachHandler(object model, Answer answer)
        {
            var vm = model as TraceToAttachViewModel;
            if (vm == null) return false;
            if (answer == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }
    }
}