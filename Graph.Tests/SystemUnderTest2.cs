using System.Collections.Generic;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public class SystemUnderTest2
    {
        public Aggregate Aggregate { get; } = new Aggregate();
        public ReadModel ReadModel { get; } = new ReadModel();
        public ClientPoller Poller { get; }

        public FakeWindowManager FakeWindowManager { get; }

        public Iit.Fibertest.TestBench.ShellViewModel ShellVm { get; }

        public SystemUnderTest2()
        {
            Poller = new ClientPoller(Aggregate.WriteModel.Db, new List<object> { ReadModel });
            FakeWindowManager = new FakeWindowManager();
            ShellVm = new Iit.Fibertest.TestBench.ShellViewModel(
                FakeWindowManager, ReadModel, new Bus(Aggregate));
        }

    }
}