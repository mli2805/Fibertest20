using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class ShellViewModel : Screen, IShell
    {
        private string _server;
        private readonly IWindowManager _windowManager;
        private readonly LoginViewModel _loginViewModel;
        private readonly ClientHeartbeat _clientHeartbeat;
        private readonly ClientPoller _clientPoller;
        private readonly OpticalEventsProvider _opticalEventsProvider;
        private readonly NetworkEventsProvider _networkEventsProvider;
        private readonly BopNetworkEventsProvider _bopNetworkEventsProvider;
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly IWcfServiceForClient _c2DWcfManager;

        public GraphReadModel GraphReadModel { get; set; }
        public MainMenuViewModel MainMenuViewModel { get; }
        public TreeOfRtuViewModel TreeOfRtuViewModel { get; }
        public TabulatorViewModel TabulatorViewModel { get; }
        public CommonStatusBarViewModel CommonStatusBarViewModel { get; }
        public OpticalEventsDoubleViewModel OpticalEventsDoubleViewModel { get; }
        public NetworkEventsDoubleViewModel NetworkEventsDoubleViewModel { get; }
        public BopNetworkEventsDoubleViewModel BopNetworkEventsDoubleViewModel { get; }

        public ShellViewModel(IniFile iniFile, IMyLog logFile, CurrentUser currentUser, IClientWcfServiceHost host,
            GraphReadModel graphReadModel, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager,
            LoginViewModel loginViewModel, ClientHeartbeat clientHeartbeat, ClientPoller clientPoller,
            MainMenuViewModel mainMenuViewModel, TreeOfRtuViewModel treeOfRtuViewModel,
            TabulatorViewModel tabulatorViewModel, CommonStatusBarViewModel commonStatusBarViewModel,
            OpticalEventsProvider opticalEventsProvider, OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
            NetworkEventsProvider networkEventsProvider, NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
            BopNetworkEventsProvider bopNetworkEventsProvider, BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel
        )
        {
            GraphReadModel = graphReadModel;
            MainMenuViewModel = mainMenuViewModel;
            TreeOfRtuViewModel = treeOfRtuViewModel;
            TabulatorViewModel = tabulatorViewModel;
            CommonStatusBarViewModel = commonStatusBarViewModel;
            OpticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            NetworkEventsDoubleViewModel = networkEventsDoubleViewModel;
            BopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _loginViewModel = loginViewModel;
            _bopNetworkEventsProvider = bopNetworkEventsProvider;
            _clientHeartbeat = clientHeartbeat;
            _clientPoller = clientPoller;
            _opticalEventsProvider = opticalEventsProvider;
            _networkEventsProvider = networkEventsProvider;
            _iniFile = iniFile;
            _logFile = logFile;
            _currentUser = currentUser;

            host.StartWcfListener();
        }


        public override void CanClose(Action<bool> callback)
        {
            _clientPollerCts.Cancel();
            _c2DWcfManager?.UnregisterClientAsync(new UnRegisterClientDto());
            _iniFile.Write(IniSection.Map, IniKey.Zoom, GraphReadModel.Zoom);
            _iniFile.Write(IniSection.Map, IniKey.CenterLatitude, GraphReadModel.CenterForIni.Lat);
            _iniFile.Write(IniSection.Map, IniKey.CenterLongitude, GraphReadModel.CenterForIni.Lng);
            _logFile.AppendLine(@"Client application finished!");
            Thread.Sleep(TimeSpan.FromMilliseconds(1000));
            base.CanClose(callback);
        }

        private readonly CancellationTokenSource _clientPollerCts = new CancellationTokenSource();

        protected override async void OnViewReady(object view)
        {
            ((App) Application.Current).ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _logFile.AssignFile(@"Client.log");

            _logFile.AppendLine(@"Client application started!");
            var isAuthenticationSuccessfull = _windowManager.ShowDialogWithAssignedOwner(_loginViewModel);
            ((App) Application.Current).ShutdownMode = ShutdownMode.OnMainWindowClose;
            if (isAuthenticationSuccessfull == true)
            {
                DisplayName = $@"Fibertest v2.0 {_currentUser.UserName} as {_currentUser.Role.ToString()}";
                var da = _iniFile.ReadDoubleAddress(11840);
                _server = da.Main.GetAddress();

                // graph MUST be read before optical/network events
                await InitializeAndRunClientPoller(_loginViewModel.GraphDbVersionOnServer);

                _opticalEventsProvider.LetsGetStarted();
                _networkEventsProvider.LetsGetStarted();
                _bopNetworkEventsProvider.LetsGetStarted();
                _clientHeartbeat.Start();
            }

            else
                TryClose();
        }

        private async Task InitializeAndRunClientPoller(Guid graphDbVersionOnServer)
        {
            _clientPoller.LoadEventSourcingCache(_server, graphDbVersionOnServer);
            await _clientPoller.LoadEventSourcingDb();

            _clientPoller.CancellationToken = _clientPollerCts.Token;
            _clientPoller.Start();
        }
    }
}