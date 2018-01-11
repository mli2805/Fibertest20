using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using PrivateReflectionUsingDynamic;
using Serilog;

namespace Iit.Fibertest.Client
{
    public partial class ShellViewModel : Screen, IShell
    {
        public ILogger Log { get; set; }

        private string _server;

        public ILifetimeScope GlobalScope;
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
            IniFile iniFile, ILogger clientLogger, IMyLog logFile, CurrentUser currentUser, IClientWcfServiceHost host)
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

            Log = clientLogger;
            Log.Information(@"Client application started!");

            host.StartWcfListener();
        }

        public override void CanClose(Action<bool> callback)
        {
            _clientPollerCts.Cancel();
            C2DWcfManager?.UnregisterClientAsync(new UnRegisterClientDto());
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

        protected override void OnViewLoaded(object view)
        {
            GraphReadModel.PropertyChanged += GraphReadModel_PropertyChanged;
        }

        private void GraphReadModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Request")
                this.AsDynamic().ComplyWithRequest(GraphReadModel.Request)
                    // This call is needed so there's no warning
                    .ConfigureAwait(false);
        }

        #region Node

        public async Task ComplyWithRequest(AddNode request)
        {
            var cmd = request;
            cmd.Id = Guid.NewGuid();
            await C2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task ComplyWithRequest(RequestAddNodeIntoFiber request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            var message =
                await C2DWcfManager.SendCommandAsObj(cmd);
            if (message != null)
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, message));
            }
        }

        public async Task ComplyWithRequest(MoveNode request)
        {
            var cmd = request;
            await C2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task ComplyWithRequest(UpdateNode request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await C2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task ComplyWithRequest(RequestRemoveNode request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            var message =
                await C2DWcfManager.SendCommandAsObj(cmd);
            if (message != null)
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, message));
            }
        }

        #endregion

        #region Fiber

        public async Task ComplyWithRequest(AddFiber request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await C2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task ComplyWithRequest(RequestAddFiberWithNodes request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            var message =
                await C2DWcfManager.SendCommandAsObj(cmd);
            if (message != null)
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, message));
            }
        }

        public async Task ComplyWithRequest(RequestUpdateFiber request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await C2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task ComplyWithRequest(RemoveFiber request)
        {
            var cmd = request;
            await C2DWcfManager.SendCommandAsObj(cmd);
        }

        #endregion

        #region RTU

        public async Task ComplyWithRequest(RequestAddRtuAtGpsLocation request)
        {
            var cmd = new AddRtuAtGpsLocation
            {
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Id = Guid.NewGuid(),
                NodeId = Guid.NewGuid()
            };
            await C2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task ComplyWithRequest(RequestUpdateRtu request)
        {
            var rtu = ReadModel.Rtus.First(r => r.NodeId == request.NodeId);
            var vm = new RtuUpdateViewModel(rtu.Id, ReadModel, C2DWcfManager);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async Task ComplyWithRequest(RequestRemoveRtu request)
        {
            var rtu = GraphReadModel.Rtus.FirstOrDefault(r => r.Node.Id == request.NodeId);
            if (rtu == null)
                return;
            var cmd = new RemoveRtu() {Id = rtu.Id};
            await C2DWcfManager.SendCommandAsObj(cmd);
        }

        #endregion

        #region Equipment

        public async Task ComplyWithRequest(RequestAddEquipmentAtGpsLocation request)
        {
            var cmd = new AddEquipmentAtGpsLocation()
            {
                Id = Guid.NewGuid(),
                NodeId = Guid.NewGuid(),
                Type = request.Type,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
            };
            await C2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task ComplyWithRequest(RequestAddEquipmentIntoNode request)
        {
            await VerboseTasks.AddEquipmentIntoNodeFullTask(request, ReadModel, _windowManager, C2DWcfManager);
        }

        public async Task ComplyWithRequest(UpdateEquipment request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await C2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task ComplyWithRequest(RemoveEquipment request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await C2DWcfManager.SendCommandAsObj(cmd);
        }

        #endregion

        #region Trace

        public Task ComplyWithRequest(RequestAddTrace request)
        {
            PrepareCommand(request);
            return Task.FromResult(0);
        }

        #endregion
    }
}