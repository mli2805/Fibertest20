using Autofac;

namespace Setup
{
    using System;
    using System.Collections.Generic;
    using Caliburn.Micro;

    public class AppBootstrapper : BootstrapperBase
    {
        SimpleContainer container;

        private ILifetimeScope _container;

        public AppBootstrapper() {
            Initialize();
        }

        protected override void Configure()
        {
//            container = new SimpleContainer();

//            container.Singleton<IWindowManager, WindowManager>();
//            container.Singleton<IEventAggregator, EventAggregator>();
//            container.PerRequest<IShell, ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            //            return container.GetInstance(service, key);
            return string.IsNullOrWhiteSpace(key) ?
                _container.Resolve(service) :
                _container.ResolveNamed(key, service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
//            return container.GetAllInstances(service);
            return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
//            container.BuildUp(instance);
            _container.InjectProperties(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacInSetup>();
            _container = builder.Build();

            var currentInstallation = _container.Resolve<CurrentInstallation>();
            currentInstallation.ProductName = "IIT Fibertest";
            currentInstallation.ProductVersion = "2.0";
            currentInstallation.BuildNumber = "1";
            currentInstallation.Revision = "777";

            DisplayRootViewFor<IShell>();
        }
    }
}