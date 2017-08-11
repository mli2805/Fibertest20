using System;
using System.Linq;
using Dto;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public class SutForTraceAttach : SystemUnderTest
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
                ShellVm.Bus.SendCommand(cmd);
            }
            else
                vm.Close();
            return true;
        }

        public OtauLeaf AttachOtauToRtu(RtuLeaf rtuLeaf, int port)
        {
            var portLeaf = (PortLeaf)rtuLeaf.ChildrenImpresario.Children[port - 1];
            FakeWindowManager.RegisterHandler(model => OtauToAttachHandler(model, rtuLeaf.Id, port, Answer.Yes));
            portLeaf.AttachOtauAction(null);
            Poller.Tick();
            return (OtauLeaf) rtuLeaf.ChildrenImpresario.Children[port - 1];
        }

        public bool RtuInitializeHandler(object model, Guid rtuId, string mainIpAddress, string reserveIpAddress, Answer answer)
        {
            var vm = model as RtuInitializeViewModel;
            if (vm == null) return false;
            if (answer == Answer.Yes)
            {
                vm.MainChannelTestViewModel.NetAddressInputViewModel.Ip4InputViewModel = new Ip4InputViewModel(mainIpAddress);
                if (reserveIpAddress != "")
                {
                    vm.IsReserveChannelEnabled = true;
                    vm.ReserveChannelTestViewModel.NetAddressInputViewModel.Ip4InputViewModel = new Ip4InputViewModel(reserveIpAddress);
                }
                if (!vm.CheckAddressUniqueness())
                    return true;

                var cmd = new InitializeRtu()
                {
                    Id = rtuId,
                    MainChannel = new NetAddress(mainIpAddress, 11832),
                    IsReserveChannelSet = reserveIpAddress != "",
                    ReserveChannel = new NetAddress(reserveIpAddress, 11832),
                    FullPortCount = 8,
                    OwnPortCount = 8,
                    Serial = @"123456"
                };
                ShellVm.Bus.SendCommand(cmd);
            }

            else
                vm.Close();
            return true;
        }
        public RtuLeaf InitializeRtu(Guid rtuId)
        {
            var rtuLeaf = (RtuLeaf)ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(rtuId);
            FakeWindowManager.RegisterHandler(model=>RtuInitializeHandler(model, rtuId, "", "", Answer.Yes));

            rtuLeaf.RtuSettingsAction(null);

            Poller.Tick();

            return rtuLeaf;
        }

        public void AttachTraceTo(Guid traceId, IPortOwner owner, int port, Answer answer)
        {
            FakeWindowManager.RegisterHandler(model => TraceToAttachHandler(model, traceId, answer));

            var portLeaf = (PortLeaf)(owner.ChildrenImpresario.Children[port - 1]);

            portLeaf.AttachFromListAction(null);
            Poller.Tick();
        }

        public RtuLeaf TraceCreatedAndRtuInitialized(out Guid traceId, out Guid rtuId)
        {
            traceId = CreateTraceRtuEmptyTerminal().Id;
            var id = traceId;
            rtuId = ReadModel.Traces.First(t => t.Id == id).RtuId;
            return InitializeRtu(rtuId);
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