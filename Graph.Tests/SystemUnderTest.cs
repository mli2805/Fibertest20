using System.Collections.Generic;
using Iit.Fibertest.Graph;

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
            Poller = new ClientPoller(Aggregate.WriteModel.Db, new List<object> { ReadModel }); 
        }
    }
}