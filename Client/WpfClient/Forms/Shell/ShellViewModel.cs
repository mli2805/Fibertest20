using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
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
        private readonly IClientWcfServiceHost _host;
        private readonly ILifetimeScope _globalScope;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly ILocalDbManager _localDbManager;

        public GraphReadModel GraphReadModel { get; set; }
        public MainMenuViewModel MainMenuViewModel { get; }
        public TreeOfRtuViewModel TreeOfRtuViewModel { get; }
        public TabulatorViewModel TabulatorViewModel { get; }
        public CommonStatusBarViewModel CommonStatusBarViewModel { get; }
        public OpticalEventsDoubleViewModel OpticalEventsDoubleViewModel { get; }
        public NetworkEventsDoubleViewModel NetworkEventsDoubleViewModel { get; }
        public BopNetworkEventsDoubleViewModel BopNetworkEventsDoubleViewModel { get; }

        public ShellViewModel(ILifetimeScope globalScope, IMyLog logFile, CurrentUser currentUser, IClientWcfServiceHost host,
            GraphReadModel graphReadModel, IWcfServiceForClient c2DWcfManager, ILocalDbManager localDbManager, IWindowManager windowManager,
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
            _c2DWcfManager = c2DWcfManager;
            _localDbManager = localDbManager;
            _windowManager = windowManager;
            _loginViewModel = loginViewModel;
            _clientHeartbeat = clientHeartbeat;
            _storedEventsLoader = storedEventsLoader;
            _clientPoller = clientPoller;
            _logFile = logFile;
            _currentUser = currentUser;
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

            _logFile.AssignFile(@"Client.log");
            _logFile.AppendLine(@"Client application started!");

            var isAuthenticationSuccessfull = _windowManager.ShowDialog(_loginViewModel);

            ((App)Application.Current).ShutdownMode = ShutdownMode.OnMainWindowClose;

            if (isAuthenticationSuccessfull == true)
            {
                TabulatorViewModel.SelectedTabIndex = 4;
                MainMenuViewModel.Initialize(_currentUser);

                await GetStoredData();
                StartCommunicationWithServer();

                IsEnabled = true;
                DisplayName =
                    $@"Fibertest v2.0 {_currentUser.UserName} as {_currentUser.Role.ToString()} [{_currentUser.ZoneTitle}]";
                TabulatorViewModel.SelectedTabIndex = 0; // the same value should be in TabulatorViewModel c-tor !!!
            }
            else
                TryClose();
        }

        public async Task GetStoredData()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                _localDbManager.Initialize(_loginViewModel.GraphDbVersionOnServer);
                _clientPoller.CurrentEventNumber = await _storedEventsLoader.Load();
            }
        }

        private void StartCommunicationWithServer()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                _clientPoller.CancellationToken = _clientPollerCts.Token;
                _clientPoller.Start(); // graph events including monitoring results events

                _host.StartWcfListener(); // Accepts only monitoring step messages and out of turn measurements results
                _clientHeartbeat.Start();
            }
        }

        public override void CanClose(Action<bool> callback)
        {
            var question = Resources.SID_Close_application_;
            var vm = new MyMessageBoxViewModel(MessageType.Confirmation, question);
            _windowManager.ShowDialogWithAssignedOwner(vm);

            if (!vm.IsAnswerPositive) return;

            base.CanClose(callback);
            Task.Factory.StartNew(() =>
            {
                _clientPollerCts.Cancel();
                _c2DWcfManager?.UnregisterClientAsync(new UnRegisterClientDto());
                _logFile.AppendLine(@"Client application finished!");
            });
        }
    }
}