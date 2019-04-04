using System;
using System.Diagnostics;
using System.Linq;
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
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class ShellViewModel : Screen, IShell
    {
        private readonly IWindowManager _windowManager;
        private readonly LoginViewModel _loginViewModel;
        private readonly StoredEventsLoader _storedEventsLoader;
        private readonly ClientPoller _clientPoller;
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly CommandLineParameters _commandLineParameters;
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
            IClientWcfServiceHost host, IWcfServiceForClient c2DWcfManager, IWcfServiceInSuperClient c2SWcfManager,
            GraphReadModel graphReadModel, ILocalDbManager localDbManager, IWindowManager windowManager,
            LoginViewModel loginViewModel, StoredEventsLoader storedEventsLoader, ClientPoller clientPoller,
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
            _storedEventsLoader = storedEventsLoader;
            _clientPoller = clientPoller;
            _logFile = logFile;
            _currentUser = currentUser;
            _currentDatacenterParameters = currentDatacenterParameters;
            _commandLineParameters = commandLineParameters;
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
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            DisplayName = @"Fibertest v"+fvi.FileVersion;

            ((App)Application.Current).ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var postfix = _commandLineParameters.IsUnderSuperClientStart ? _commandLineParameters.ClientOrdinal.ToString() : "";
            _logFile.AssignFile($@"client{postfix}.log");
            _logFile.AppendLine(@"Client application started!");

            if (_commandLineParameters.IsUnderSuperClientStart)
            {
                _iniFile.WriteServerAddresses(new DoubleAddress() { Main = _commandLineParameters.ServerNetAddress });
                _iniFile.Write(IniSection.General, IniKey.Culture, _commandLineParameters.SuperClientCulture);
                _iniFile.Write(IniSection.ClientLocalAddress, IniKey.TcpPort, (int)TcpPorts.ClientListenTo + _commandLineParameters.ClientOrdinal);
                _iniFile.Write(IniSection.Miscellaneous, IniKey.IsGraphVisibleOnStart, false);
                await _loginViewModel.RegisterClientAsync(_commandLineParameters.Username, _commandLineParameters.Password, true);
            }
            else
                _windowManager.ShowDialog(_loginViewModel);

            ((App)Application.Current).ShutdownMode = ShutdownMode.OnMainWindowClose;

            if (_loginViewModel.IsRegistrationSuccessful)
            {
                TabulatorViewModel.SelectedTabIndex = 4;

                await GetAlreadyStoredInCacheAndOnServerData();
                StartRegularCommunicationWithServer();
                if (_commandLineParameters.IsUnderSuperClientStart)
                    await Task.Factory.StartNew(() => NotifySuperClientImReady(_commandLineParameters.ClientOrdinal));
                IsEnabled = true;
                TreeOfRtuViewModel.CollapseAll();
                const string separator = @"    >>    ";
                var server = $@"{separator}{_currentDatacenterParameters.ServerTitle} ({_currentDatacenterParameters.ServerIp}) v{_currentDatacenterParameters.DatacenterVersion}";
                var user   = $@"{separator}{_currentUser.UserName} ({_currentUser.Role.ToString()})";
                var zone   = $@"{separator}[{_currentUser.ZoneTitle}]";
                DisplayName = DisplayName + $@" {server} {user} {zone}";
                TabulatorViewModel.SelectedTabIndex = 0; // the same value should be in TabulatorViewModel c-tor !!!
                var driveInfo = await _c2DWcfManager.GetDiskSpace();
                var totalSize = $@"{driveInfo.TotalSize / (1024.0 * 1024 * 1024):#.0}Gb";
                var freeSpace = $@"{driveInfo.AvailableFreeSpace / (1024.0 * 1024 * 1024):#.0}Gb";
                var dataSize = $@"{driveInfo.DataSize / (1024.0 * 1024 * 1024):0.0}Gb";
                _logFile.AppendLine($@"Database's size is {totalSize},  free space is {freeSpace},   database size is {dataSize}");
            }
            else
            {
                if (_commandLineParameters.IsUnderSuperClientStart)
                    await Task.Factory.StartNew(() => NotifySuperclientLoadingFailed(_commandLineParameters.ClientOrdinal));
                TryClose();
            }
        }

        private async Task NotifySuperClientImReady(int postfix)
        {
            _logFile.AppendLine(@"Notify super-client I'm ready");
            Thread.Sleep(TimeSpan.FromMilliseconds(1));
            var isStateOk = !OpticalEventsDoubleViewModel.ActualOpticalEventsViewModel.Rows.Any() &&
                            !NetworkEventsDoubleViewModel.ActualNetworkEventsViewModel.Rows.Any() &&
                            !BopNetworkEventsDoubleViewModel.ActualBopNetworkEventsViewModel.Rows.Any();
            await _c2SWcfManager.ClientLoadingResult(postfix, true, isStateOk);
        }

        private async Task NotifySuperclientLoadingFailed(int postfix)
        {
            await _c2SWcfManager.ClientLoadingResult(postfix, false, false);
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

                _host.StartWcfListener(); // Accepts only monitoring step messages and client's measurement results
            }
        }

        public override async void CanClose(Action<bool> callback)
        {
            if (_loginViewModel.IsRegistrationSuccessful)
            {
                var question = Resources.SID_Close_application_;
                var vm = new MyMessageBoxViewModel(MessageType.Confirmation, question);
                _windowManager.ShowDialogWithAssignedOwner(vm);

                if (!vm.IsAnswerPositive) return;
            }

            _clientPollerCts.Cancel();
            _logFile.AppendLine(@"Client application finished.");

            if (_c2DWcfManager == null)
                base.CanClose(callback);
            else
            {
                 await _c2DWcfManager.UnregisterClientAsync(new UnRegisterClientDto()).ContinueWith(ttt => { Environment.Exit(Environment.ExitCode); });
            }
        }
    }
}