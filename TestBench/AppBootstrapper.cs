using System;
using System.Collections.Generic;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench {
    public class AppBootstrapper : BootstrapperBase
    {
        private ILifetimeScope container;

        public AppBootstrapper() {
            Initialize();
        }

        protected override void Configure() {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacEventSourcing>();
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<ShellViewModel>().As<IShell>();

            container = builder.Build();

        }


        protected override object GetInstance(Type service, string key)
        {
            return string.IsNullOrWhiteSpace(key) ?
                container.Resolve(service) :
                container.ResolveNamed(key, service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            container.InjectProperties(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e) {
            DisplayRootViewFor<IShell>();
        }
    }
}