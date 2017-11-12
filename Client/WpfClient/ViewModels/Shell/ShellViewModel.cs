﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;
using PrivateReflectionUsingDynamic;
using Serilog;

namespace Iit.Fibertest.Client
{
    public partial class ShellViewModel : Screen, IShell
    {
        public ILogger Log { get; set; }

        private readonly Guid _clientId;
        private int _userId;
        private readonly IWindowManager _windowManager;
        private readonly ClientHeartbeat _clientHeartbeat;
        private readonly IMyLog _logFile;
        public IWcfServiceForClient C2DWcfManager { get; }

        public ReadModel ReadModel { get; }
        public MainMenuViewModel MainMenuViewModel { get; set; }
        public TreeOfRtuModel TreeOfRtuModel { get; }
        public TreeOfRtuViewModel TreeOfRtuViewModel { get; set; }
        public GraphReadModel GraphReadModel { get; set; }

        private bool? _isAuthenticationSuccessfull;

        private Visibility _sysEventsVisibility;
        private int _selectedTabIndex;

        public Visibility SysEventsVisibility
        {
            get { return _sysEventsVisibility; }
            set
            {
                if (value == _sysEventsVisibility) return;
                _sysEventsVisibility = value;
                NotifyOfPropertyChange();
            }
        }

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

        public ShellViewModel(ReadModel readModel, TreeOfRtuModel treeOfRtuModel, IWcfServiceForClient c2DWcfManager,
                GraphReadModel graphReadModel, IWindowManager windowManager, ClientHeartbeat clientHeartbeat,
                ILogger clientLogger, IniFile iniFile, IMyLog logFile, ClientWcfService clientWcfService, IClientWcfServiceHost host)
        {
            ReadModel = readModel;
            TreeOfRtuModel = treeOfRtuModel;
            TreeOfRtuModel.PostOffice.PropertyChanged += PostOffice_PropertyChanged;
            MainMenuViewModel = new MainMenuViewModel(windowManager);
            TreeOfRtuViewModel = new TreeOfRtuViewModel(treeOfRtuModel);
            GraphReadModel = graphReadModel;
            GraphReadModel.MapVisibility = Visibility.Visible;
            SysEventsVisibility = Visibility.Collapsed;
            _selectedTabIndex = 1;
            C2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _clientHeartbeat = clientHeartbeat;

            Guid.TryParse(iniFile.Read(IniSection.General, IniKey.ClientGuidOnServer, Guid.NewGuid().ToString()), out _clientId);

            _logFile = logFile;

            Log = clientLogger;
            Log.Information(@"Client application started!");

            host.StartWcfListener();
            clientWcfService.MessageReceived += ClientWcfService_MessageReceived;
        }

        private void ClientWcfService_MessageReceived(object e)
        {
            var dto = e as ListOfRtuWithChangedAvailabilityDto;
            if (dto != null)
            {
                _logFile.AppendLine(@"ShellViewModel recieved RTU availability changed");
                return;
            }

            var dto1 = e as MonitoringResultDto;
            if (dto1 != null)
            {
                _logFile.AppendLine(@"ShellViewModel recieved monitoring result");
            }
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
            vm.ClientId = _clientId;
            _isAuthenticationSuccessfull = _windowManager.ShowDialog(vm);
            ((App)Application.Current).ShutdownMode = ShutdownMode.OnMainWindowClose;
            if (_isAuthenticationSuccessfull == true)
            {
                _userId = vm.UserId;
                StartPolling();
                _clientHeartbeat.Start();
            }

            else
                TryClose();
        }

        private void StartPolling()
        {
            var clientPoller = IoC.Get<ClientPoller>();
            GC.KeepAlive(new DispatcherTimer(
                TimeSpan.FromSeconds(1),
                DispatcherPriority.Background,
                (s, e) => clientPoller.Tick(),
                Dispatcher.CurrentDispatcher));
            clientPoller.Tick();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"Fibertest v2.0";
            GraphReadModel.PropertyChanged += GraphReadModel_PropertyChanged;
        }

        private void GraphReadModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Request")
                this.AsDynamic().ComplyWithRequest(GraphReadModel.Request)
                    // This call is needed so there's no warning
                    .ConfigureAwait(false);
        }

        public void ChangeGisVisibility()
        {
            GraphReadModel.MapVisibility = GraphReadModel.MapVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            SysEventsVisibility = GraphReadModel.MapVisibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public void TestTraceState()
        {
            var vm = new TraceStateViewModel();
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
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
                _windowManager.ShowDialog(new NotificationViewModel(Resources.SID_Error, message));
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
                _windowManager.ShowDialog(new NotificationViewModel(Resources.SID_Error, message));
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
                _windowManager.ShowDialog(new NotificationViewModel(Resources.SID_Error, message));
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
            //            var cmd = PrepareCommand(request);
            //            if (cmd == null)
            //                return;
            var cmd = new UpdateRtu() { Id = request.Id };
            await C2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task ComplyWithRequest(RequestRemoveRtu request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
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