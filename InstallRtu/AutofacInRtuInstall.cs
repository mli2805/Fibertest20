using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.InstallLib;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.InstallRtu
{
    public sealed class AutofacInRtuInstall : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ShellViewModel>().As<IShell>();

            builder.RegisterType<LogFile>().As<IMyLog>().SingleInstance();
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<CurrentRtuInstallation>().SingleInstance();

            builder.RegisterType<LicenseAgreementViewModel>().SingleInstance();
            builder.RegisterType<InstallationFolderViewModel>().SingleInstance();
            builder.RegisterType<ProcessProgressViewModel>().SingleInstance();

            builder.RegisterType<InstallationLanguageViewModel>().SingleInstance();

            builder.RegisterType<RtuInstallManager>().SingleInstance();
            builder.RegisterType<SetupRtuManagerOperations>().SingleInstance();
            builder.RegisterType<SetupUninstallOperations>().SingleInstance();

        }
    }
}
