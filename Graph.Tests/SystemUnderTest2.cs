using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SystemUnderTest2
    {
        public ClientPoller Poller { get; }
        public FakeWindowManager FakeWindowManager { get; }
        public ShellViewModel ShellVm { get; }

        public SystemUnderTest2()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacEventSourcing>();
            builder.RegisterModule<AutofacUi>();
            builder.RegisterType<FakeWindowManager>().As<IWindowManager>().SingleInstance();

            var container = builder.Build();
            Poller = container.Resolve<ClientPoller>();
            FakeWindowManager =(FakeWindowManager) container.Resolve<IWindowManager>();
            ShellVm =(ShellViewModel) container.Resolve<IShell>();
        }

        public bool FiberWithNodesAdditionHandler(object model, int count, EquipmentType type, Answer answer)
        {
            var vm = model as FiberWithNodesAddViewModel;
            if (vm == null) return false;
            vm.Count = count;
            vm.SetSelectedType(type);
            if (answer == Answer.Yes)
                vm.Ok();
            else
                vm.Cancel();
            return true;
        }
    }

}