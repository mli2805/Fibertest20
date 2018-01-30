using System;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public class SutForTraceAttach : SutForBaseRefs
    {
        public bool OtauToAttachHandler(object model, Guid rtuId, int masterPort, Answer answer)
        {
            var vm = model as OtauToAttachViewModel;
            if (vm == null) return false;
            if (answer == Answer.Yes)
            {
                if (!vm.CheckAddressUniqueness())
                    return true;
                var cmd = new AttachOtau()
                {
                    Id = Guid.NewGuid(),
                    RtuId = rtuId,
                    MasterPort = masterPort,
                    Serial = @"123456",
                    PortCount = 16,
                    NetAddress = new NetAddress(),
                };
                ShellVm.C2DWcfManager.SendCommandAsObj(cmd).Wait();
            }
            else
                vm.Close();
            return true;
        }

        public OtauLeaf AttachOtauToRtu(RtuLeaf rtuLeaf, int port)
        {
            var portLeaf = (PortLeaf)rtuLeaf.ChildrenImpresario.Children[port - 1];
            FakeWindowManager.RegisterHandler(model => OtauToAttachHandler(model, rtuLeaf.Id, port, Answer.Yes));
            PortLeafActions.AttachOtauAction(portLeaf);
            Poller.EventSourcingTick().Wait();
            return (OtauLeaf)rtuLeaf.ChildrenImpresario.Children[port - 1];
        }

       

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