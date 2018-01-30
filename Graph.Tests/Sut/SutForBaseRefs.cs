using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Graph.Tests
{
    public class SutForBaseRefs : SystemUnderTest
    {
      
        public bool RtuInitializeHandler(object model, Guid rtuId, string mainIpAddress, string reserveIpAddress, string waveLenght, Answer answer)
        {
            if (!(model is RtuInitializeViewModel vm)) return false;
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

                var treeOfAcceptableParameters = new TreeOfAcceptableMeasParams();
                treeOfAcceptableParameters.Units.Add(waveLenght, new BranchOfAcceptableMeasParams());
                var cmd = new InitializeRtu()
                {
                    Id = rtuId,
                    MainChannel = new NetAddress(mainIpAddress, TcpPorts.RtuListenTo),
                    IsReserveChannelSet = reserveIpAddress != "",
                    ReserveChannel = new NetAddress(reserveIpAddress, TcpPorts.RtuListenTo),
                    FullPortCount = 8,
                    OwnPortCount = 8,
                    Serial = @"123456",
                    OtauNetAddress = new NetAddress(mainIpAddress, 23),
                    Version = @"2.0.1.0",
                    AcceptableMeasParams = treeOfAcceptableParameters,
                };
                ShellVm.C2DWcfManager.SendCommandAsObj(cmd).Wait();
            }

            else
                vm.Close();
            return true;
        }

        public bool RtuInitializeHandler2(object model, Guid rtuId, string mainIpAddress, string reserveIpAddress, Answer answer)
        {
            if (!(model is RtuInitializeViewModel vm)) return false;
            if (answer == Answer.Yes)
            {
                vm.MainChannelTestViewModel.NetAddressInputViewModel.Ip4InputViewModel = new Ip4InputViewModel(mainIpAddress);
                if (reserveIpAddress != "")
                {
                    vm.IsReserveChannelEnabled = true;
                    vm.ReserveChannelTestViewModel.NetAddressInputViewModel.Ip4InputViewModel = new Ip4InputViewModel(reserveIpAddress);
                }

                vm.InitializeRtu();
            }
            else
                vm.Close();
            return true;
        }

        public RtuLeaf InitializeRtu(Guid rtuId, string mainIpAddress = "", string reserveIpAddress = "", string waveLength = "SM1625")
        {
            var rtu = ReadModel.Rtus.First(r => r.Id == rtuId);

            var rtuLeaf = (RtuLeaf)ShellVm.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(rtuId);

            FakeWindowManager.RegisterHandler(model => RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            ShellVm.ComplyWithRequest(new RequestUpdateRtu() { Id = rtuId, NodeId = rtu.NodeId }).Wait();
            Poller.EventSourcingTick().Wait();

            FakeWindowManager.RegisterHandler(m => m is MyMessageBoxViewModel);
            FakeWindowManager.RegisterHandler(model => RtuInitializeHandler2(model, rtuId, mainIpAddress, reserveIpAddress, Answer.Yes));
            rtuLeaf.MyContextMenu.FirstOrDefault(i => i.Header == Resources.SID_Network_settings)?.Command.Execute(rtuLeaf);


//            FakeWindowManager.RegisterHandler(model => RtuInitializeHandler(model, rtuId, "", "", waveLength, Answer.Yes));
//            RtuLeafActions.InitializeRtu(rtuLeaf);
            Poller.EventSourcingTick().Wait();
            rtuLeaf.TreeOfAcceptableMeasParams.Units.ContainsKey(waveLength).Should().BeTrue();
            return rtuLeaf;
        }
     

        public bool BaseRefAssignHandler(object model, Guid traceId, string precisePath, string fastPath, string aditionalPath, Answer answer)
        {
            var vm = model as BaseRefsAssignViewModel;
            if (vm == null) return false;
            if (answer == Answer.Yes)
            {
                if (precisePath == "")
                    vm.ClearPathToPrecise();
                else if (precisePath != null)
                    vm.PreciseBaseFilename = precisePath;

                if (fastPath == "")
                    vm.ClearPathToFast();
                else if (fastPath != null)
                    vm.FastBaseFilename = fastPath;

                if (aditionalPath == "")
                    vm.ClearPathToAdditional();
                else if (aditionalPath != null)
                    vm.AdditionalBaseFilename = aditionalPath;

                var cmd = new AssignBaseRef()
                {
                    TraceId = traceId,
                    BaseRefs = vm.GetBaseRefChangesList(),
                };
                ShellVm.C2DWcfManager.SendCommandAsObj(cmd).Wait();
            }
            else
                vm.Cancel();
            return true;
        }

        public bool BaseRefAssignHandler2(object model, string precisePath, string fastPath, string aditionalPath, Answer answer)
        {
            if (!(model is BaseRefsAssignViewModel vm)) return false;
            if (answer == Answer.Yes)
            {
                if (precisePath == "")
                    vm.ClearPathToPrecise();
                else if (precisePath != null)
                    vm.PreciseBaseFilename = precisePath;

                if (fastPath == "")
                    vm.ClearPathToFast();
                else if (fastPath != null)
                    vm.FastBaseFilename = fastPath;

                if (aditionalPath == "")
                    vm.ClearPathToAdditional();
                else if (aditionalPath != null)
                    vm.AdditionalBaseFilename = aditionalPath;

                vm.Save().Wait();
            }
            else
                vm.Cancel();
            return true;
        }
    }
}