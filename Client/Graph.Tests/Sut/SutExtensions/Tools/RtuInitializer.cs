using System;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;


namespace Graph.Tests
{
    public static class RtuInitializer
    {
        public static RtuLeaf SetNameAndAskInitializationRtu(this SystemUnderTest sut, Guid rtuId, 
            string mainIpAddress = "1.1.1.1", string reserveIpAddress = "2.2.2.2", int port = 11842)
        {
            var rtu = sut.ReadModel.Rtus.First(r => r.Id == rtuId);
            var rtuLeaf = (RtuLeaf)sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(rtuId);

            sut.FakeWindowManager.RegisterHandler(model => 
                sut.RtuUpdateHandler(model, @"Some title for RTU", @"doesn't matter", Answer.Yes));
            sut.GraphReadModel.GrmRtuRequests.UpdateRtu(new RequestUpdateRtu() { RtuId = rtuId, NodeId = rtu.NodeId });
            sut.Poller.EventSourcingTick().Wait();

            sut.FakeWindowManager.RegisterHandler(m => m is MyMessageBoxViewModel);
            sut.FakeWindowManager.RegisterHandler(model => 
                sut.RtuInitializeHandler(model, mainIpAddress, reserveIpAddress, port));
            rtuLeaf.MyContextMenu.First(i => i?.Header == Resources.SID_Network_settings).Command.Execute(rtuLeaf);

            sut.Poller.EventSourcingTick().Wait();
            return rtuLeaf;
        }

        public static bool RtuInitializeHandler(this SystemUnderTest sut, object model, 
            string mainIpAddress, string reserveIpAddress, int port = 11842, Answer answer = Answer.Yes)
        {
            if (!(model is RtuInitializeViewModel vm)) return false;
            if (answer == Answer.Yes)
            {
                vm.FullModel.MainChannelTestViewModel.NetAddressInputViewModel.Ip4InputViewModel = new Ip4InputViewModel(mainIpAddress);
                vm.FullModel.MainChannelTestViewModel.NetAddressInputViewModel.Port = port;
                if (!string.IsNullOrEmpty(reserveIpAddress))
                {
                    vm.FullModel.IsReserveChannelEnabled = true;
                    vm.FullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.Ip4InputViewModel = new Ip4InputViewModel(reserveIpAddress);
                    vm.FullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.Port = port;
                }

                vm.InitializeRtu().Wait();
            }
            else
                vm.Close();
            return true;
        }
    }
}