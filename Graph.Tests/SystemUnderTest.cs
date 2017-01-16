using System.Collections.Generic;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WpfClient.ViewModels;

namespace Graph.Tests
{
    public class SystemUnderTest
    {
        public Aggregate Aggregate { get; } = new Aggregate();
        public ReadModel ReadModel { get; } = new ReadModel();
        public ClientPoller Poller { get; }
        public int CurrentEventNumber => Poller.CurrentEventNumber;
        public MapViewModel MapVm { get; }
        public FakeWindowManager FakeWindowManager { get; }

        public SystemUnderTest()
        {
            Poller = new ClientPoller(Aggregate.WriteModel.Db, new List<object> { ReadModel });
            FakeWindowManager = new FakeWindowManager();
            MapVm = new MapViewModel(Aggregate, ReadModel, FakeWindowManager);
        }
    }
}