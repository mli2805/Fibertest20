namespace KadastrLoader 
{
    using System;
    using System.Collections.Generic;
    using Caliburn.Micro;
    using Autofac;
    using System.Reflection;
    using Iit.Fibertest.UtilsLib;
    using System.Threading;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Markup;

    public class AppBootstrapper : BootstrapperBase 
    {
        private ILifetimeScope _container;

        public AppBootstrapper() 
        {
            Initialize();
        }

        protected override void Configure() 
        {
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

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            SomeInitialActions();
            DisplayRootViewFor<IShell>();
        }

        private void SomeInitialActions()
        {
            var iniFileName = $@"kadastr.ini";

            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacKadastr>();

            var iniFile = new IniFile();
            iniFile.AssignFile(iniFileName);
            builder.RegisterInstance(iniFile);

            _container = builder.Build();

            var currentCulture = iniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(currentCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(currentCulture);

            // Ensure the current culture passed into bindings 
            // is the OS culture. By default, WPF uses en-US 
            // as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            yield return typeof(KadastrLoaderView).Assembly; // this Assembly (.exe)
        }
    }
}