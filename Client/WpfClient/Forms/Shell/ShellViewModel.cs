using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class ShellViewModel : Screen, IShell
    {
        private readonly IWindowManager _windowManager;
        private readonly LoginViewModel _loginViewModel;
        private readonly StoredEventsLoader _storedEventsLoader;
        private readonly Heartbeater _heartbeater;
        private readonly ClientPoller _clientPoller;
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly CommandLineParameters _commandLineParameters;
        private readonly IClientWcfServiceHost _host;
        private readonly ILifetimeScope _globalScope;
        private readonly IniFile _iniFile;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWcfServiceCommonC2D _commonC2DWcfManager;
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
            IClientWcfServiceHost host, IWcfServiceDesktopC2D c2DWcfManager, IWcfServiceCommonC2D commonC2DWcfManager,
            IWcfServiceInSuperClient c2SWcfManager,
            GraphReadModel graphReadModel, ILocalDbManager localDbManager, IWindowManager windowManager,
            LoginViewModel loginViewModel, StoredEventsLoader storedEventsLoader, 
            Heartbeater heartbeater, ClientPoller clientPoller,
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
            _commonC2DWcfManager = commonC2DWcfManager;
            _c2SWcfManager = c2SWcfManager;
            _localDbManager = localDbManager;
            _windowManager = windowManager;
            _loginViewModel = loginViewModel;
            _storedEventsLoader = storedEventsLoader;
            _heartbeater = heartbeater;
            _clientPoller = clientPoller;
            _logFile = logFile;
            _currentUser = currentUser;
            _currentDatacenterParameters = currentDatacenterParameters;
            _commandLineParameters = commandLineParameters;
            _host = host;
        }


        private readonly CancellationTokenSource _clientPollerCts = new CancellationTokenSource();
        private readonly CancellationTokenSource _heartbeaterCts = new CancellationTokenSource();

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

        private string _backgroundMessage;
        public string BackgroundMessage
        {
            get => _backgroundMessage;
            set
            {
                if (value == _backgroundMessage) return;
                _backgroundMessage = value;
                NotifyOfPropertyChange();
            }
        }

        protected override async void OnViewLoaded(object view)
        {
            BackgroundMessage = Resources.SID_Data_is_loading;
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
                _iniFile.Write(IniSection.ClientLocalAddress, IniKey.ClientOrdinal, _commandLineParameters.ClientOrdinal);
                _iniFile.Write(IniSection.Miscellaneous, IniKey.IsGraphVisibleOnStart, false);
                await _loginViewModel.RegisterClientAsync(_commandLineParameters.Username, _commandLineParameters.Password,
                    _commandLineParameters.ConnectionId, true, _commandLineParameters.ClientOrdinal);
            }
            else
                _windowManager.ShowDialog(_loginViewModel);

            ((App)Application.Current).ShutdownMode = ShutdownMode.OnMainWindowClose;

            if (_loginViewModel.IsRegistrationSuccessful)
            {
                TabulatorViewModel.SelectedTabIndex = 4;

                SetDisplayName();
                StartSendHeartbeats();
                await GetAlreadyStoredInCacheAndOnServerData();
                StartRegularCommunicationWithServer();
                if (_commandLineParameters.IsUnderSuperClientStart)
                    await Task.Factory.StartNew(() => NotifySuperClientImReady(_commandLineParameters.ClientOrdinal));
                IsEnabled = true;
                TreeOfRtuViewModel.CollapseAll();
                 TabulatorViewModel.SelectedTabIndex = 0; // the same value should be in TabulatorViewModel c-tor !!!
                var unused = await CheckFreeSpaceThreshold();
            }
            else
            {
                if (_commandLineParameters.IsUnderSuperClientStart)
                    await Task.Factory.StartNew(() => NotifySuperclientLoadingFailed(_commandLineParameters.ClientOrdinal));
                TryClose();
            }
        }

        private void SetDisplayName()
        {
            const string separator = @"    >>    ";
            var server =
                $@"{separator}{_currentDatacenterParameters.ServerTitle} ({_currentDatacenterParameters.ServerIp}) v{_currentDatacenterParameters.DatacenterVersion}";
            var user = $@"{separator}{_currentUser.UserName} ({_currentUser.Role.ToString()})";
            var zone = $@"{separator}[{_currentUser.ZoneTitle}]";
            DisplayName = DisplayName + $@" {server} {user} {zone}";
        }

        private async Task<bool> CheckFreeSpaceThreshold()
        {
            var driveInfo = await _c2DWcfManager.GetDiskSpaceGb();
            var totalSize = $@"Database drive's size: {driveInfo.TotalSize:0.0}Gb";
            var freeSpace = $@"free space: {driveInfo.AvailableFreeSpace:0.0}Gb";
            var dataSize = $@"database size: {driveInfo.DataSize:0.0}Gb";
            var threshold = $@"threshold: {driveInfo.FreeSpaceThreshold:0.0}Gb";
            _logFile.AppendLine($@"{totalSize},  {dataSize},  {freeSpace},  {threshold}");
            if (driveInfo.AvailableFreeSpace < driveInfo.FreeSpaceThreshold)
            {
                var str = new List<string>()
                {
                    Resources.SID_Free_space_threshold_exceeded_, "",
                    $@"{Resources.SID_Db_drive_free_space}  {driveInfo.AvailableFreeSpace:0.0} Gb",
                    $@"{Resources.SID_Free_space_threshold}  {driveInfo.FreeSpaceThreshold:0.0} Gb", "",
                    $@"{Resources.SID_Db_drive_total_size}  {driveInfo.TotalSize:0.0} Gb",
                    $@"{Resources.SID_Fibertest_data_size}  {driveInfo.DataSize:0.000} Gb",
                };
                var vm = new MyMessageBoxViewModel(MessageType.Information, str, 2);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }

            return true;
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
                _localDbManager.Initialize();
                var cleaningResult = await _storedEventsLoader.ClearCacheIfDoesnotMatchDb();
                if (cleaningResult == CacheClearResult.ClearedSuccessfully)
                    BackgroundMessage = Resources.SID_Loading_data_after_DB_recovery__optimization_;
                _clientPoller.CurrentEventNumber = 
                    await _storedEventsLoader.TwoComponentLoading(cleaningResult == CacheClearResult.ClearedSuccessfully 
                                                                  || cleaningResult == CacheClearResult.CacheNotFound);
            }
        }

        private void StartSendHeartbeats()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                _heartbeater.CancellationTokenSource = _heartbeaterCts;
                _heartbeater.Start();
            }
        }

        private void StartRegularCommunicationWithServer()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                _clientPoller.CancellationTokenSource = _clientPollerCts;
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

            _heartbeaterCts.Cancel();
            _clientPollerCts.Cancel();
            _logFile.AppendLine(@"Client application finished.");

            if (_c2DWcfManager == null)
                base.CanClose(callback);
            else
            {
                 await _commonC2DWcfManager.UnregisterClientAsync(new UnRegisterClientDto(){ConnectionId = _currentUser.ConnectionId}).
                     ContinueWith(ttt => { Environment.Exit(Environment.ExitCode); });
            }
        }
    }
}