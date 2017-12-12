using System;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public class SutForTraceCleanRemove : SutForTraceAttach
    {
        public Guid TraceId1, TraceId2;

        public void CreateTwoTraces()
        {
            ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            Poller.EventSourcingTick().Wait();
            var nodeForRtuId = ReadModel.Rtus.Last().NodeId;

            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            Poller.EventSourcingTick().Wait();
            var a1 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.EventSourcingTick().Wait();
            var a2 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.EventSourcingTick().Wait();
            var b2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = a1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = a1, Node2 = a2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = a1, Node2 = b2 }).Wait();
            Poller.EventSourcingTick().Wait();

            TraceId1 = DefineTrace(a2, nodeForRtuId).Id;
            TraceId2 = DefineTrace(b2, nodeForRtuId).Id;
        }
    }
}