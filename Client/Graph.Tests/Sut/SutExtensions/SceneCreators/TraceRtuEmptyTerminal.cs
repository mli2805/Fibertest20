﻿using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;

namespace Graph.Tests
{
    public static class TraceRtuEmptyTerminal
    {
        public static void ApplyDemoLicense(this SystemUnderTest sut)
        {
            sut.WcfServiceDesktopC2D.SendCommandAsObj(new ApplyLicense()
            {
                LicenseId = Guid.NewGuid(),
                Owner = @"RtuAtGpsLocationAddedSteps 1 RTU",
                RtuCount = new LicenseParameter(){Value = 1, ValidUntil = DateTime.MaxValue},
            }).Wait();
            sut.Poller.EventSourcingTick().Wait();
        }

        public static Iit.Fibertest.Graph.Trace CreateTraceRtuEmptyTerminal(this SystemUnderTest sut, string title = @"some title")
        {
            sut.FakeWindowManager.RegisterHandler(model => sut.RtuUpdateHandler(model, @"something", @"doesn't matter", Answer.Yes));
            sut.GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 });
            sut.Poller.EventSourcingTick().Wait();
            var nodeForRtuId = sut.ReadModel.Rtus.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation()
            {
                Type = EquipmentType.EmptyNode,
                Latitude = 55.1,
                Longitude = 30.1
            }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var firstNodeId = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation()
            {
                Type = EquipmentType.Terminal,
                Latitude = 55.2,
                Longitude = 30.2
            }).Wait();
            sut.Poller.EventSourcingTick().Wait();
            var secondNodeId = sut.ReadModel.Nodes.Last().NodeId;

            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = nodeForRtuId, NodeId2 = firstNodeId }).Wait();
            sut.GraphReadModel.GrmFiberRequests.AddFiber(new AddFiber() { NodeId1 = firstNodeId, NodeId2 = secondNodeId }).Wait();
            sut.Poller.EventSourcingTick().Wait();

            return sut.DefineTrace(secondNodeId, nodeForRtuId, title);
        }
    }
}