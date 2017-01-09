using System;
using System.Collections.Generic;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using PrivateReflectionUsingDynamic;

namespace Graph.Tests
{
    public class SystemUnderTest
    {
        public Aggregate Aggregate { get; } = new Aggregate();
        public ReadModel ReadModel { get; } = new ReadModel();
        public ClientPoller Poller { get; }
        public int CurrentEventNumber => Poller.CurrentEventNumber;

        public SystemUnderTest()
        {
            Poller = new ClientPoller(Aggregate.Db, new List<object> { ReadModel }); 
        }

        public void RemoveFiber(Guid fiberId)
        {
            var cmd = new RemoveFiber() {Id = fiberId};
            Aggregate.When(cmd);
            Poller.Tick();
        }

        public void AttachTrace(AttachTrace cmd)
        {
            Aggregate.When(cmd);
            Poller.Tick();
        }

        public void DetachTrace(DetachTrace cmd)
        {
            Aggregate.When(cmd);
            Poller.Tick();
        }
    }
}