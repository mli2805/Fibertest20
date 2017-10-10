﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
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

            var clientPoller = _container.Resolve<ClientPoller>();
            GC.KeepAlive(new DispatcherTimer(
                TimeSpan.FromSeconds(1), 
                DispatcherPriority.Background, 
                (s, e) => clientPoller.Tick(),
                Dispatcher.CurrentDispatcher));
            clientPoller.Tick();
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