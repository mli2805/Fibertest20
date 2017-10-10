using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Caliburn.Micro;

namespace Iit.Fibertest.Client {
    public class AppBootstrapper : BootstrapperBase
    {
        private ILifetimeScope _container;

        public AppBootstrapper() {
            Initialize();
        }

        protected override void Configure()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacClient>();
            _container = builder.Build();
        }


        protected override object GetInstance(Type service, string key)
        {
            return string.IsNullOrWhiteSpace(key) ?
                _container.Resolve(service) :
                _container.ResolveNamed(key, service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            _container.InjectProperties(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e) {
            DisplayRootViewFor<IShell>();
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            yield return typeof(ShellView).Assembly; // this Assembly (.exe)
            yield return typeof(WpfCommonViews.RftsEventsView).Assembly; // WpfCommonViews
        }
    }
}