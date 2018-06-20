﻿using Autofac;
using Caliburn.Micro;
using Setup;

namespace Iit.Fibertest.Setup
{
    public sealed class AutofacInSetup : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ShellViewModel>().As<IShell>();
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<CurrentInstallation>().SingleInstance();

            builder.RegisterType<LicenseAgreementViewModel>().SingleInstance();
            builder.RegisterType<InstallationFolderViewModel>().SingleInstance();
            builder.RegisterType<InstTypeChoiceViewModel>().SingleInstance();
            builder.RegisterType<ProcessProgressViewModel>().SingleInstance();

            builder.RegisterType<InstallationLanguageViewModel>().SingleInstance();

            builder.RegisterType<SetupManager>().SingleInstance();
            builder.RegisterType<SetupClientOperations>().SingleInstance();
            builder.RegisterType<SetupDataCenterOperations>().SingleInstance();
            builder.RegisterType<SetupRtuManagerOperations>().SingleInstance();
            builder.RegisterType<SetupUninstallOperations>().SingleInstance();

        }
    }
}
