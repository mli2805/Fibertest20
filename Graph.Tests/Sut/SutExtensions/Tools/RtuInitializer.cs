using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;

namespace Graph.Tests
{
    public static class RtuInitializer
    {
        public static RtuLeaf InitializeRtu(this SystemUnderTest sut, Guid rtuId, 
            string mainIpAddress = "", string reserveIpAddress = "", string waveLength = "SM1625")
        {
            var rtu = sut.ReadModel.Rtus.First(r => r.Id == rtuId);
            var rtuLeaf = (RtuLeaf)sut.TreeOfRtuViewModel.TreeOfRtuModel.Tree.GetById(rtuId);

            sut.FakeWindowManager.RegisterHandler(model => sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            sut.GraphReadModel.GrmRtuRequests.UpdateRtu(new RequestUpdateRtu() { RtuId = rtuId, NodeId = rtu.NodeId });
            sut.Poller.EventSourcingTick().Wait();

            sut.FakeWindowManager.RegisterHandler(m => m is MyMessageBoxViewModel);
            sut.FakeWindowManager.RegisterHandler(model => sut.RtuInitializeHandler2(model, mainIpAddress, reserveIpAddress, Answer.Yes));
            rtuLeaf.MyContextMenu.FirstOrDefault(i => i.Header == Resources.SID_Network_settings)?.Command.Execute(rtuLeaf);

            sut.Poller.EventSourcingTick().Wait();

            if (waveLength != @"SM1625")
            {
                rtuLeaf.TreeOfAcceptableMeasParams.Units.Clear();
                rtuLeaf.TreeOfAcceptableMeasParams.Units.Add(waveLength, new BranchOfAcceptableMeasParams());
            }
            rtuLeaf.TreeOfAcceptableMeasParams.Units.ContainsKey(waveLength).Should().BeTrue();
            return rtuLeaf;
        }

        public static bool RtuInitializeHandler2(this SystemUnderTest sut, object model, 
            string mainIpAddress, string reserveIpAddress, Answer answer)
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
    }
}