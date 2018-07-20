using Caliburn.Micro;
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

    
    }
}