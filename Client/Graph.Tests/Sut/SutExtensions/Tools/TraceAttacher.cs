﻿using System;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.StringResources;

namespace Graph.Tests
{
    public static class TraceAttacher
    {
        public static TraceLeaf AttachTraceTo(this SystemUnderTest sut, Guid traceId, IPortOwner owner, int port, Answer answer)
        {
            sut.FakeWindowManager.RegisterHandler(model => TraceToAttachHandler(model, traceId, answer));

            var portLeaf = (PortLeaf)(owner.ChildrenImpresario.Children[port - 1]);
            portLeaf.MyContextMenu.First(i => i.Header == Resources.SID_Attach_trace).Command.Execute(portLeaf);
            sut.Poller.EventSourcingTick().Wait();
            return (TraceLeaf)sut.TreeOfRtuViewModel.TreeOfRtuModel.GetById(traceId);
        }

        private static bool TraceToAttachHandler(object model, Guid traceId, Answer answer)
        {
            if (!(model is TraceToAttachViewModel vm)) return false;
            vm.SelectedTrace = vm.Choices.First(t => t.TraceId == traceId);
            if (answer == Answer.Yes)
                vm.FullAttach();
            else
                vm.Cancel();
            return true;
        }
    }
}