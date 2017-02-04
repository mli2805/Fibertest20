namespace Graph.Tests
{
    public class SystemUnderTest2
    {
        public FakeWindowManager FakeWindowManager { get; }

        public Iit.Fibertest.TestBench.ShellViewModel ShellVm { get; }

        public SystemUnderTest2()
        {
            FakeWindowManager = new FakeWindowManager();
            ShellVm = new Iit.Fibertest.TestBench.ShellViewModel(FakeWindowManager);
        }

    }
}