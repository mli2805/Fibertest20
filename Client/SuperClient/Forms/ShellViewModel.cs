using System;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.SuperClient
{
    public class ShellViewModel : Screen, IShell
    {
        private readonly IMyLog _logFile;
        private readonly SuperClientWcfServiceHost _superClientWcfServiceHost;
        public ServersViewModel ServersViewModel { get; set; }
        public GasketViewModel GasketViewModel { get; set; }
     

        public ShellViewModel(IMyLog logFile, SuperClientWcfServiceHost superClientWcfServiceHost, 
            ServersViewModel serversViewModel, GasketViewModel gasketViewModel)
        {
            _logFile = logFile;
            _superClientWcfServiceHost = superClientWcfServiceHost;
            ServersViewModel = serversViewModel;
            GasketViewModel = gasketViewModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Fibertest 2.0 Superclient";
            _logFile.AssignFile(@"sc.log");
            _logFile.AppendLine(@"Super-Client application started!");
            _superClientWcfServiceHost.StartWcfListener();
        }

        public override void CanClose(Action<bool> callback)
        {
            var res = MessageBox.Show(Resources.SID_Close_application_, "Confirmation", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
                base.CanClose(callback);
        }
    }
}