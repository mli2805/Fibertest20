using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
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
        private readonly ReadyEventsLoader _readyEventsLoader;
        private readonly ClientPoller _clientPoller;
        private readonly OpticalEventsProvider _opticalEventsProvider;
        private readonly NetworkEventsProvider _networkEventsProvider;
        private readonly BopNetworkEventsProvider _bopNetworkEventsProvider;
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly ILifetimeScope _globalScope;
        private readonly IWcfServiceForClient _c2DWcfManager;

        public GraphReadModel GraphReadModel { get; set; }
        public MainMenuViewModel MainMenuViewModel { get; }
        public TreeOfRtuViewModel TreeOfRtuViewModel { get; }
        public TabulatorViewModel TabulatorViewModel { get; }
        public CommonStatusBarViewModel CommonStatusBarViewModel { get; }
        public OpticalEventsDoubleViewModel OpticalEventsDoubleViewModel { get; }
        public NetworkEventsDoubleViewModel NetworkEventsDoubleViewModel { get; }
        public BopNetworkEventsDoubleViewModel BopNetworkEventsDoubleViewModel { get; }

        public ShellViewModel(ILifetimeScope globalScope, IniFile iniFile, IMyLog logFile, CurrentUser currentUser, IClientWcfServiceHost host,
            GraphReadModel graphReadModel, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager,
            LoginViewModel loginViewModel, ClientHeartbeat clientHeartbeat, ReadyEventsLoader readyEventsLoader, ClientPoller clientPoller,
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
            _globalScope = globalScope;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _loginViewModel = loginViewModel;
            _bopNetworkEventsProvider = bopNetworkEventsProvider;
            _clientHeartbeat = clientHeartbeat;
            _readyEventsLoader = readyEventsLoader;
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
            base.CanClose(callback);
            Task.Factory.StartNew(() =>
            {
                _clientPollerCts.Cancel();
                _c2DWcfManager?.UnregisterClientAsync(new UnRegisterClientDto());
                _logFile.AppendLine(@"Client application finished!");
            });
        }

        private readonly CancellationTokenSource _clientPollerCts = new CancellationTokenSource();

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        protected override async void OnViewLoaded(object view)
        {
            ((App)Application.Current).ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _logFile.AssignFile(@"Client.log");

            _logFile.AppendLine(@"Client application started!");
            TabulatorViewModel.SelectedTabIndex = 3;
            var isAuthenticationSuccessfull = _windowManager.ShowDialog(_loginViewModel);
            ((App)Application.Current).ShutdownMode = ShutdownMode.OnMainWindowClose;
            if (isAuthenticationSuccessfull == true)
            {
                using (_globalScope.Resolve<IWaitCursor>())
                {
                    TabulatorViewModel.SelectedTabIndex = 4;
                    MainMenuViewModel.Initialize(_currentUser);
                    var da = _iniFile.ReadDoubleAddress(11840);
                    _server = da.Main.GetAddress();

                    var localDbManager = (LocalDbManager)_globalScope.Resolve<ILocalDbManager>();
                    localDbManager.Initialize(_server, _loginViewModel.GraphDbVersionOnServer);
                    _clientPoller.CurrentEventNumber = await _readyEventsLoader.Load();
                    _clientPoller.CancellationToken = _clientPollerCts.Token;
                    _clientPoller.Start();

                    _opticalEventsProvider.LetsGetStarted();
                    _networkEventsProvider.LetsGetStarted();
                    _bopNetworkEventsProvider.LetsGetStarted();
                    _clientHeartbeat.Start();

                    IsEnabled = true;
                    DisplayName = $@"Fibertest v2.0 {_currentUser.UserName} as {_currentUser.Role.ToString()} [{_currentUser.ZoneTitle}]";
                    TabulatorViewModel.SelectedTabIndex = 3;
                }
            }

            else
                TryClose();
        }
    }
}