using System;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Graph.Tests
{
    public static class OtauAttacher
    {
        private static bool OtauToAttachHandler(object model, Guid rtuId, int masterPort, Answer answer)
        {
            if (!(model is OtauToAttachViewModel vm)) return false;
            if (answer == Answer.Yes)
            {
                vm.Initialize(rtuId, masterPort);
                vm.AttachOtau().Wait();
            }
            else
                vm.Close();
            return true;
        }

        public static OtauLeaf AttachOtau(this SystemUnderTest sut, RtuLeaf rtuLeaf, int masterPort)
        {
            var portLeaf = (PortLeaf)rtuLeaf.ChildrenImpresario.Children[masterPort - 1];
            sut.FakeWindowManager.RegisterHandler(model => OtauToAttachHandler(model, rtuLeaf.Id, masterPort, Answer.Yes));
            portLeaf.MyContextMenu.First(i => i.Header == Resources.SID_Attach_optical_switch).Command.Execute(portLeaf);
            sut.Poller.EventSourcingTick().Wait();
            return (OtauLeaf)rtuLeaf.ChildrenImpresario.Children[masterPort - 1];
        }
    }
}