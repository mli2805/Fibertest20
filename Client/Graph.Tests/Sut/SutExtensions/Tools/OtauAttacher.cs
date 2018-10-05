using System;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Graph.Tests
{
    public static class OtauAttacher
    {
        private static bool OtauToAttachHandler(object model, Guid rtuId, int masterPort, 
            string otauIp = "1.1.1.1", int otauTcpPort = 11834, Answer answer = Answer.Yes)
        {
            if (!(model is OtauToAttachViewModel vm)) return false;
            if (answer == Answer.Yes)
            {
                vm.Initialize(rtuId, masterPort);
                vm.NetAddressInputViewModel = new NetAddressInputViewModel(new NetAddress(otauIp, otauTcpPort), true);
                vm.AttachOtau().Wait();
            }
            else
                vm.Close();
            return true;
        }

        public static OtauLeaf AttachOtau(this SystemUnderTest sut, RtuLeaf rtuLeaf, int masterPort,
            string otauIp = "1.1.1.1", int otauTcpPort = 11834)
        {
            var portLeaf = (PortLeaf)rtuLeaf.ChildrenImpresario.Children[masterPort - 1];
            sut.FakeWindowManager.RegisterHandler(model => OtauToAttachHandler(model, rtuLeaf.Id, masterPort, otauIp, otauTcpPort));
            portLeaf.MyContextMenu.First(i => i.Header == Resources.SID_Attach_optical_switch).Command.Execute(portLeaf);
            sut.Poller.EventSourcingTick().Wait();
            var otauLeaf = rtuLeaf.ChildrenImpresario.Children[masterPort - 1] as OtauLeaf;
            return otauLeaf;
        }
    }
}