using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
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

        private readonly IWindowManager _windowManager;
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
        public MainMenuViewModel MainMenuViewModel { get; set; }
        public TreeOfRtuModel TreeOfRtuModel { get; }
        public TreeOfRtuViewModel TreeOfRtuViewModel { get; set; }
        public GraphReadModel GraphReadModel { get; set; }
        public OpticalEventsDoubleViewModel OpticalEventsDoubleViewModel { get; set; }
        public NetworkEventsDoubleViewModel NetworkEventsDoubleViewModel { get; set; }
        public BopNetworkEventsDoubleViewModel BopNetworkEventsDoubleViewModel { get; set; }
        public CommonStatusBarViewModel CommonStatusBarViewModel { get; set; }

        private bool? _isAuthenticationSuccessfull;

        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (value == _selectedTabIndex) return;
                _selectedTabIndex = value;
                ChangeGisVisibility();
            }
        }

        public ShellViewModel(ReadModel readModel, TreeOfRtuModel treeOfRtuModel, GraphReadModel graphReadModel,
                IWcfServiceForClient c2DWcfManager, IWindowManager windowManager,
                NetworkEventsDoubleViewModel networkEventsDoubleViewModel, NetworkEventsProvider networkEventsProvider,
                OpticalEventsDoubleViewModel opticalEventsDoubleViewModel, OpticalEventsProvider opticalEventsProvider,
                BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel, BopNetworkEventsProvider bopNetworkEventsProvider,
                CommonStatusBarViewModel commonStatusBarViewModel,
                ClientHeartbeat clientHeartbeat, ClientPoller clientPoller, 
                IniFile iniFile, ILogger clientLogger, IMyLog logFile, CurrentUser currentUser, IClientWcfServiceHost host)
        {
            ReadModel = readModel;
            TreeOfRtuModel = treeOfRtuModel;
            TreeOfRtuModel.PostOffice.PropertyChanged += PostOffice_PropertyChanged;
            MainMenuViewModel = new MainMenuViewModel(windowManager);
            TreeOfRtuViewModel = new TreeOfRtuViewModel(treeOfRtuModel);
            GraphReadModel = graphReadModel;
            GraphReadModel.MapVisibility = Visibility.Collapsed;
            OpticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            OpticalEventsDoubleViewModel.OpticalEventsVisibility = Visibility.Visible;
            NetworkEventsDoubleViewModel = networkEventsDoubleViewModel;
            NetworkEventsDoubleViewModel.NetworkEventsVisibility = Visibility.Collapsed;
            BopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;
            CommonStatusBarViewModel = commonStatusBarViewModel;
            BopNetworkEventsDoubleViewModel.BopNetworkEventsVisibility = Visibility.Collapsed;
            _selectedTabIndex = 0;
            C2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
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
            // if user cancelled login view - _c2DWcfManager would be null
            C2DWcfManager?.UnregisterClientAsync(new UnRegisterClientDto());
            _logFile.AppendLine(@"Client application finished!");
            base.CanClose(callback);
        }

        private void PostOffice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Message")
                GraphReadModel.ProcessMessage(((PostOffice)sender).Message);
        }

        protected override void OnViewReady(object view)
        {
            ((App)Application.Current).ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _logFile.AssignFile(@"Client.log");
            _logFile.AppendLine(@"Client application started!");
            var vm = IoC.Get<LoginViewModel>();
            _isAuthenticationSuccessfull = _windowManager.ShowDialogWithAssignedOwner(vm);
            ((App)Application.Current).ShutdownMode = ShutdownMode.OnMainWindowClose;
            if (_isAuthenticationSuccessfull == true)
            {
                DisplayName = $@"Fibertest v2.0 {_currentUser.UserName} as {_currentUser.Role.ToString()}";
                var da = _iniFile.ReadDoubleAddress(11840);
                _server = da.Main.GetAddress();

                _opticalEventsProvider.LetsGetStarted();
                _networkEventsProvider.LetsGetStarted();
                _bopNetworkEventsProvider.LetsGetStarted();
                _clientPoller.LoadEventSourcingCache(_server);
                _clientPoller.Start();
                _clientHeartbeat.Start();
            }

            else
                TryClose();
        }

        protected override void OnViewLoaded(object view)
        {
           // DisplayName = $@"Fibertest v2.0 {_currentUser.UserName}";
            GraphReadModel.PropertyChanged += GraphReadModel_PropertyChanged;
        }

        private void GraphReadModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Request")
                this.AsDynamic().ComplyWithRequest(GraphReadModel.Request)
                    // This call is needed so there's no warning
                    .ConfigureAwait(false);
        }

        private void ChangeGisVisibility()
        {
            switch (_selectedTabIndex)
            {
                case 0:
                    OpticalEventsDoubleViewModel.OpticalEventsVisibility = Visibility.Visible;
                    NetworkEventsDoubleViewModel.NetworkEventsVisibility = Visibility.Collapsed;
                    BopNetworkEventsDoubleViewModel.BopNetworkEventsVisibility = Visibility.Collapsed;
                    GraphReadModel.MapVisibility = Visibility.Collapsed;
                    break;
                case 1:
                    OpticalEventsDoubleViewModel.OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsDoubleViewModel.NetworkEventsVisibility = Visibility.Visible;
                    BopNetworkEventsDoubleViewModel.BopNetworkEventsVisibility = Visibility.Collapsed;
                    GraphReadModel.MapVisibility = Visibility.Collapsed;
                    break;
                case 2:
                    OpticalEventsDoubleViewModel.OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsDoubleViewModel.NetworkEventsVisibility = Visibility.Collapsed;
                    BopNetworkEventsDoubleViewModel.BopNetworkEventsVisibility = Visibility.Visible;
                    GraphReadModel.MapVisibility = Visibility.Collapsed;
                    break;
                case 3:
                    OpticalEventsDoubleViewModel.OpticalEventsVisibility = Visibility.Collapsed;
                    NetworkEventsDoubleViewModel.NetworkEventsVisibility = Visibility.Collapsed;
                    BopNetworkEventsDoubleViewModel.BopNetworkEventsVisibility = Visibility.Collapsed;
                    GraphReadModel.MapVisibility = Visibility.Visible;
                    break;
            }
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
                _windowManager.ShowDialogWithAssignedOwner(new NotificationViewModel(Resources.SID_Error, message));
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
                _windowManager.ShowDialogWithAssignedOwner(new NotificationViewModel(Resources.SID_Error, message));
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
                _windowManager.ShowDialogWithAssignedOwner(new NotificationViewModel(Resources.SID_Error, message));
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

        #region Rtu
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
            var cmd = new RemoveRtu() { Id = rtu.Id };
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