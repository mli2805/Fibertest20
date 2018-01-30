using System;
using System.Linq;
using Iit.Fibertest.Client;

namespace Graph.Tests
{
    public class SutForTraceAttach : SystemUnderTest
    {
        public void AttachTraceTo(Guid traceId, IPortOwner owner, int port, Answer answer)
        {
            FakeWindowManager.RegisterHandler(model => TraceToAttachHandler(model, traceId, answer));

            var portLeaf = (PortLeaf)(owner.ChildrenImpresario.Children[port - 1]);

            PortLeafActions.AttachFromListAction(portLeaf);
            Poller.EventSourcingTick().Wait();
        }

        public RtuLeaf TraceCreatedAndRtuInitialized(out Guid traceId, out Guid rtuId, string traceTitle = "some title")
        {
            traceId = CreateTraceRtuEmptyTerminal(traceTitle).Id;
            var id = traceId;
            rtuId = ReadModel.Traces.First(t => t.Id == id).RtuId;
            return this.InitializeRtu(rtuId, @"192.168.96.58");
        }

        public bool TraceToAttachHandler(object model, Guid traceId, Answer answer)
        {
            var vm = model as TraceToAttachViewModel;
            if (vm == null) return false;
            vm.SelectedTrace = vm.Choices.First(t => t.Id == traceId);
            if (answer == Answer.Yes)
                vm.Attach();
            else
                vm.Cancel();
            return true;
        }

    }
}