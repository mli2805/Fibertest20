using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;
using PrivateReflectionUsingDynamic;
using Serilog;

namespace Iit.Fibertest.Client
{
    public partial class ShellViewModel : Screen, IShell
    {
        public ILogger Log { get; set; }
        private readonly IniFile _iniFile;

        public Bus Bus { get; }
        private readonly Guid _clientId;
        private readonly IWindowManager _windowManager;
        private readonly IMyLog _logFile;
        private C2DWcfManager _c2DWcfManager;

        public ReadModel ReadModel { get; }
        public MainMenuViewModel MainMenuViewModel { get; set; }
        public TreeOfRtuModel TreeOfRtuModel { get; }
        public TreeOfRtuViewModel TreeOfRtuViewModel { get; set; }
        public GraphReadModel GraphReadModel { get; set; }

        private bool? _isAuthenticationSuccessfull;
        public AdministrativeDb AdministrativeDb { get; set; }

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

        public ShellViewModel(ReadModel readModel, TreeOfRtuModel treeOfRtuModel, Bus bus, 
                AdministrativeDb administrativeDb, GraphReadModel graphReadModel, IWindowManager windowManager, 
                ILogger clientLogger, IniFile iniFile, IMyLog logFile, IClientWcfServiceHost host)
        {
            ReadModel = readModel;
            TreeOfRtuModel = treeOfRtuModel;
            TreeOfRtuModel.PostOffice.PropertyChanged += PostOffice_PropertyChanged;
            MainMenuViewModel = new MainMenuViewModel(windowManager);
            TreeOfRtuViewModel = new TreeOfRtuViewModel(treeOfRtuModel);
            Bus = bus;
            AdministrativeDb = administrativeDb;
            GraphReadModel = graphReadModel;
            GraphReadModel.MapVisibility = Visibility.Visible;
            SysEventsVisibility = Visibility.Collapsed;
            _selectedTabIndex = 1;
            _windowManager = windowManager;

            _iniFile = iniFile;
            Guid.TryParse(_iniFile.Read(IniSection.General, IniKey.ClientGuidOnServer, Guid.NewGuid().ToString()), out _clientId);

            _logFile = logFile;

            Log = clientLogger;
            Log.Information(@"Client application started!");

            host.StartWcfListener();
        }

     
        public override void CanClose(Action<bool> callback)
        {
            // if user cancelled login view - _c2DWcfManager would be null
            _c2DWcfManager?.UnregisterClientAsync(new UnRegisterClientDto());
            Save();
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
            var vm = new LoginViewModel(_clientId, _windowManager, _iniFile, _logFile);
            _isAuthenticationSuccessfull = _windowManager.ShowDialog(vm);
            ((App)Application.Current).ShutdownMode = ShutdownMode.OnMainWindowClose;
            if (_isAuthenticationSuccessfull != true)
                TryClose();

            _c2DWcfManager = IoC.Get<C2DWcfManager>();
            var dcServiceAddresses = _iniFile.ReadDoubleAddress(11840);
            _c2DWcfManager = new C2DWcfManager(dcServiceAddresses, _iniFile, _logFile, _clientId);
            TreeOfRtuModel.C2DWcfManager = _c2DWcfManager;
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
        public void Save()
        {
            AdministrativeDb.Save();
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
            await Bus.SendCommand(cmd);
        }

        public async Task ComplyWithRequest(RequestAddNodeIntoFiber request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            var message = await Bus.SendCommand(cmd);
            if (message != null)
            {
                _windowManager.ShowDialog(new NotificationViewModel(Resources.SID_Error, message));
            }
        }

        public async Task ComplyWithRequest(MoveNode request)
        {
            var cmd = request;
            await Bus.SendCommand(cmd);
        }

        public async Task ComplyWithRequest(UpdateNode request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await Bus.SendCommand(cmd);
        }

        public async Task ComplyWithRequest(RequestRemoveNode request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            var message = await Bus.SendCommand(cmd);
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
            await Bus.SendCommand(cmd);
        }

        public async Task ComplyWithRequest(RequestAddFiberWithNodes request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            var message = await Bus.SendCommand(cmd);
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
            await Bus.SendCommand(cmd);
        }

        public async Task ComplyWithRequest(RemoveFiber request)
        {
            var cmd = request;
            await Bus.SendCommand(cmd);
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
            await Bus.SendCommand(cmd);
        }

        public async Task ComplyWithRequest(RequestUpdateRtu request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await Bus.SendCommand(cmd);
        }

        public async Task ComplyWithRequest(RequestRemoveRtu request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await Bus.SendCommand(cmd);
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
            await Bus.SendCommand(cmd);
        }

        public async Task ComplyWithRequest(RequestAddEquipmentIntoNode request)
        {
            await VerboseTasks.AddEquipmentIntoNodeFullTask(request, ReadModel, _windowManager, Bus, _c2DWcfManager);
        }
        public async Task ComplyWithRequest(UpdateEquipment request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await Bus.SendCommand(cmd);
        }

        public async Task ComplyWithRequest(RemoveEquipment request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await Bus.SendCommand(cmd);
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

    public interface IClientWcfServiceHost
    {
        void StartWcfListener();
    }

    public sealed class ClientWcfServiceHost : IClientWcfServiceHost
    {
        private readonly ServiceHost _wcfHost = new ServiceHost(typeof(ClientWcfService));
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        public ClientWcfServiceHost(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }

        private void ClientWcfService_MessageReceived(object e)
        {
            if (e is MonitoringResultDto)
                _logFile.AppendLine(@"Moniresult happened");
        }

        public void StartWcfListener()
        {
            ClientWcfService.ClientLog = _logFile;
            ClientWcfService.MessageReceived += ClientWcfService_MessageReceived;

            try
            {
                _wcfHost.AddServiceEndpoint(typeof(IClientWcfService), 
                    WcfFactory.CreateDefaultNetTcpBinding(_iniFile), 
                    WcfFactory.CombineUriString(@"localhost", (int)TcpPorts.ClientListenTo, @"ClientWcfService"));
                _wcfHost.Open();
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                throw;
            }
        }

    }
}