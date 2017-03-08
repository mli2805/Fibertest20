using System;
using System.Linq;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SutForTraceAttach : SystemUnderTest
    {
        public bool OtauToAttachHandler(object model, Answer answer)
        {
            var vm = model as OtauToAttachViewModel;
            if (vm == null) return false;
            if (answer == Answer.Yes)
                vm.Attach();
            else
                vm.Close();
            return true;
        }

        public OtauLeaf AttachOtauToRtu(RtuLeaf rtuLeaf, int port)
        {
            var portLeaf = (PortLeaf)rtuLeaf.Children[port - 1];
            FakeWindowManager.RegisterHandler(model => OtauToAttachHandler(model, Answer.Yes));
            portLeaf.AttachOtauAction(null);
            Poller.Tick();
            return (OtauLeaf) rtuLeaf.Children[port - 1];
        }

        public bool RtuInitializeHandler(object model, Answer answer)
        {
            var vm = model as RtuInitializeViewModel;
            if (vm == null) return false;
            if (answer == Answer.Yes)
                vm.Initialize();
            else
                vm.Close();
            return true;
        }
        public RtuLeaf InitializeRtu(Guid rtuId)
        {
            var rtuLeaf = (RtuLeaf)ShellVm.MyLeftPanelViewModel.TreeReadModel.Tree.GetById(rtuId);
            FakeWindowManager.RegisterHandler(model=>RtuInitializeHandler(model, Answer.Yes));

            rtuLeaf.RtuSettingsAction(null);

            Poller.Tick();
            return rtuLeaf;
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
            InitializeRtu(rtuId);
        }

        public bool TraceToAttachHandler(object model, Answer answer)
        {
            var vm = model as TraceToAttachViewModel;
            if (vm == null) return false;
            if (answer == Answer.Yes)
                vm.Attach();
            else
                vm.Cancel();
            return true;
        }

    }
}