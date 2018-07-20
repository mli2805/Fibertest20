using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.SuperClientWcfServiceInterface;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class ShellViewModel : Screen, IShell
    {
        private readonly IWindowManager _windowManager;
        private readonly LoginViewModel _loginViewModel;
        private readonly ClientHeartbeat _clientHeartbeat;
        private readonly StoredEventsLoader _storedEventsLoader;
        private readonly ClientPoller _clientPoller;
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly CommandLineParameters _commandLineParameters;
        private readonly ClientWcfService _clientWcfService;
        private readonly IClientWcfServiceHost _host;
        private readonly ILifetimeScope _globalScope;
        private readonly IniFile _iniFile;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWcfServiceInSuperClient _c2SWcfManager;
        private readonly ILocalDbManager _localDbManager;

        public GraphReadModel GraphReadModel { get; set; }
        public MainMenuViewModel MainMenuViewModel { get; }
        public TreeOfRtuViewModel TreeOfRtuViewModel { get; }
        public TabulatorViewModel TabulatorViewModel { get; }
        public CommonStatusBarViewModel CommonStatusBarViewModel { get; }
        public OpticalEventsDoubleViewModel OpticalEventsDoubleViewModel { get; }
        public NetworkEventsDoubleViewModel NetworkEventsDoubleViewModel { get; }
        public BopNetworkEventsDoubleViewModel BopNetworkEventsDoubleViewModel { get; }

        public ShellViewModel(ILifetimeScope globalScope, IniFile iniFile, IMyLog logFile, CurrentUser currentUser,
            CurrentDatacenterParameters currentDatacenterParameters, CommandLineParameters commandLineParameters,
            ClientWcfService clientWcfService, IClientWcfServiceHost host,
            GraphReadModel graphReadModel, IWcfServiceForClient c2DWcfManager, IWcfServiceInSuperClient c2SWcfManager,
            ILocalDbManager localDbManager, IWindowManager windowManager,
            LoginViewModel loginViewModel, ClientHeartbeat clientHeartbeat, StoredEventsLoader storedEventsLoader, ClientPoller clientPoller,
            MainMenuViewModel mainMenuViewModel, TreeOfRtuViewModel treeOfRtuViewModel,
            TabulatorViewModel tabulatorViewModel, CommonStatusBarViewModel commonStatusBarViewModel,
             OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
             NetworkEventsDoubleViewModel networkEventsDoubleViewModel,
             BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel
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
            _iniFile = iniFile;
            _c2DWcfManager = c2DWcfManager;
            _c2SWcfManager = c2SWcfManager;
            _localDbManager = localDbManager;
            _windowManager = windowManager;
            _loginViewModel = loginViewModel;
            _clientHeartbeat = clientHeartbeat;
            _storedEventsLoader = storedEventsLoader;
            _clientPoller = clientPoller;
            _logFile = logFile;
            _currentUser = currentUser;
            _currentDatacenterParameters = currentDatacenterParameters;
            _commandLineParameters = commandLineParameters;
            _clientWcfService = clientWcfService;
            _host = host;
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
            TabulatorViewModel.MessageVisibility = Visibility.Collapsed;
            DisplayName = @"Fibertest v2.0";

            ((App)Application.Current).ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var postfix = _iniFile.Read(IniSection.Client, IniKey.ClientOrdinal, "");
            _logFile.AssignFile($@"client{postfix}.log");
            _logFile.AppendLine(@"Client application started!");

            if (_commandLineParameters.IsUnderSuperClientStart)
            {
                _iniFile.WriteServerAddresses(new DoubleAddress(){Main = _commandLineParameters.ServerNetAddress});
                _iniFile.Write(IniSection.Client, IniKey.ClientOrdinal, _commandLineParameters.ClientOrdinal);
                _iniFile.Write(IniSection.ClientLocalAddress, IniKey.TcpPort, (int)TcpPorts.ClientListenTo + _commandLineParameters.ClientOrdinal);
                await _loginViewModel.RegisterClientAsync(_commandLineParameters.Username, _commandLineParameters.Password);
            }
            else
                _windowManager.ShowDialog(_loginViewModel);

            ((App)Application.Current).ShutdownMode = ShutdownMode.OnMainWindowClose;

            if (_loginViewModel.IsRegistrationSuccessful)
            {
                TabulatorViewModel.SelectedTabIndex = 4;
                MainMenuViewModel.Initialize(_currentUser);

                await GetAlreadyStoredInCacheAndOnServerData();
                StartRegularCommunicationWithServer();
                if (postfix != "")
                    NotifySuperClientImReady(postfix);
                IsEnabled = true;
                DisplayName =
                    $@"Fibertest v2.0 {_currentUser.UserName} as {_currentUser.Role.ToString()} [{_currentUser.ZoneTitle}]";
                TabulatorViewModel.SelectedTabIndex = 0; // the same value should be in TabulatorViewModel c-tor !!!
            }
            else
                TryClose();
        }

        private void NotifySuperClientImReady(string postfix)
        {
            _logFile.AppendLine(@"Notify superclient I'm ready");
            if (int.TryParse(postfix, out int number))
            {
                _c2SWcfManager.ClientLoaded(number);
            }
        }

        public async Task GetAlreadyStoredInCacheAndOnServerData()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                _localDbManager.Initialize(_currentDatacenterParameters.GraphDbVersionId);
                _clientPoller.CurrentEventNumber = await _storedEventsLoader.Load();
            }
        }

        private void StartRegularCommunicationWithServer()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                _clientPoller.CancellationToken = _clientPollerCts.Token;
                _clientPoller.Start(); // graph events including monitoring results events

                _host.StartWcfListener(); // Accepts only monitoring step messages and out of turn measurements results
                _clientWcfService.PropertyChanged += _clientWcfService_PropertyChanged;
                _clientHeartbeat.Start();
            }
        }

        private void _clientWcfService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
//            if (e.PropertyName == "Cmd")
//            {
//                if (_clientWcfService.Cmd == 1)
//                    this.ActivateWith(null);
//                if (_clientWcfService.Cmd == 99)
//                    TryClose();
//            }
        }

        public override void CanClose(Action<bool> callback)
        {
            var question = Resources.SID_Close_application_;
            var vm = new MyMessageBoxViewModel(MessageType.Confirmation, question);
            _windowManager.ShowDialogWithAssignedOwner(vm);

            if (!vm.IsAnswerPositive) return;

            _clientPollerCts.Cancel();
            _logFile.AppendLine(@"Client application finished.");

            if (_c2DWcfManager == null)
                base.CanClose(callback);
            else
                _c2DWcfManager.UnregisterClientAsync(new UnRegisterClientDto()).ContinueWith(ttt => { base.CanClose(callback); });
        }
    }
}