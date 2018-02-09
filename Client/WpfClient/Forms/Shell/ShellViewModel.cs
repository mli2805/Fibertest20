using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class 
        
    ShellViewModel : Screen, IShell
    {
        private string _server;
        public readonly ILifetimeScope GlobalScope;
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
        public IWcfServiceForClient C2DWcfManager { get; }

        public ReadModel ReadModel { get; }
        public MainMenuViewModel MainMenuViewModel { get; }
        public TreeOfRtuModel TreeOfRtuModel { get; }
        public TreeOfRtuViewModel TreeOfRtuViewModel { get; set; }
        public GraphReadModel GraphReadModel { get; set; }
        public OpticalEventsDoubleViewModel OpticalEventsDoubleViewModel { get; set; }
        public NetworkEventsDoubleViewModel NetworkEventsDoubleViewModel { get; set; }
        public BopNetworkEventsDoubleViewModel BopNetworkEventsDoubleViewModel { get; set; }
        public TabulatorViewModel TabulatorViewModel { get; }
        public CommonStatusBarViewModel CommonStatusBarViewModel { get; set; }

        private bool? _isAuthenticationSuccessfull;

        public ShellViewModel(ILifetimeScope globalScope, ReadModel readModel, TreeOfRtuModel treeOfRtuModel, GraphReadModel graphReadModel,
            MainMenuViewModel mainMenuViewModel,
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager,
            LoginViewModel loginViewModel, 
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel, NetworkEventsProvider networkEventsProvider,
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel, OpticalEventsProvider opticalEventsProvider,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel,
            BopNetworkEventsProvider bopNetworkEventsProvider, 
            TabulatorViewModel tabulatorViewModel,
            CommonStatusBarViewModel commonStatusBarViewModel,
            ClientHeartbeat clientHeartbeat, ClientPoller clientPoller,
            IniFile iniFile, IMyLog logFile, CurrentUser currentUser, IClientWcfServiceHost host)
        {
            ReadModel = readModel;
            TreeOfRtuModel = treeOfRtuModel;
            MainMenuViewModel = mainMenuViewModel;
            TreeOfRtuViewModel = new TreeOfRtuViewModel(treeOfRtuModel);
            GraphReadModel = graphReadModel;
            OpticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            NetworkEventsDoubleViewModel = networkEventsDoubleViewModel;
            BopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            TabulatorViewModel = tabulatorViewModel;
            CommonStatusBarViewModel = commonStatusBarViewModel;
            C2DWcfManager = c2DWcfManager;
            GlobalScope = globalScope;
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
            C2DWcfManager?.UnregisterClientAsync(new UnRegisterClientDto());
            _iniFile.Write(IniSection.Map, IniKey.Zoom, GraphReadModel.Zoom);
            _iniFile.Write(IniSection.Map, IniKey.CenterLatitude, GraphReadModel.CenterForIni.Lat);
            _iniFile.Write(IniSection.Map, IniKey.CenterLongitude, GraphReadModel.CenterForIni.Lng);
            _logFile.AppendLine(@"Client application finished!");
            Thread.Sleep(TimeSpan.FromMilliseconds(400));
            base.CanClose(callback);
        }

        private readonly CancellationTokenSource _clientPollerCts = new CancellationTokenSource();

        protected override async void OnViewReady(object view)
        {
            ((App) Application.Current).ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _logFile.AssignFile(@"Client.log");

            _logFile.AppendLine(@"Client application started!");
            _isAuthenticationSuccessfull = _windowManager.ShowDialogWithAssignedOwner(_loginViewModel);
            ((App) Application.Current).ShutdownMode = ShutdownMode.OnMainWindowClose;
            if (_isAuthenticationSuccessfull == true)
            {
                DisplayName = $@"Fibertest v2.0 {_currentUser.UserName} as {_currentUser.Role.ToString()}";
                var da = _iniFile.ReadDoubleAddress(11840);
                _server = da.Main.GetAddress();

                // graph MUST be read before optical/network events
                await InitializeAndRunClientPoller();

                _opticalEventsProvider.LetsGetStarted();
                _networkEventsProvider.LetsGetStarted();
                _bopNetworkEventsProvider.LetsGetStarted();
                _clientHeartbeat.Start();
            }

            else
                TryClose();
        }

        private async Task InitializeAndRunClientPoller()
        {
            _clientPoller.LoadEventSourcingCache(_server);
            await _clientPoller.LoadEventSourcingDb();

            _clientPoller.CancellationToken = _clientPollerCts.Token;
            _clientPoller.Start();
        }
    }
}