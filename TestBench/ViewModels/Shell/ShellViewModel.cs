using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.StringResources;
using PrivateReflectionUsingDynamic;
using Serilog;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel : Screen, IShell
    {
        public LeftPanelViewModel MyLeftPanelViewModel { get; set; }

        public ILogger Log { get; set; }

        private readonly IWindowManager _windowManager;
        public GraphVm GraphVm { get; set; } = new GraphVm();
        public ReadModel ReadModel { get; }
        public Bus Bus { get; }

        public Db LocalDb { get; set; }
        public ShellViewModel(ReadModel readModel, TreeReadModel treeReadModel, Bus bus, Db db, IWindowManager windowManager, ILogger clientLogger)
        {
            ReadModel = readModel;
            MyLeftPanelViewModel = new LeftPanelViewModel(treeReadModel);
            Bus = bus;
            LocalDb = db;
            _windowManager = windowManager;

            Log = clientLogger;
            Log.Information(@"Client started!");
        }

        public void Save()
        {
            LocalDb.Save();
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            GraphVm.PropertyChanged += GraphVm_PropertyChanged;
        }

        private void GraphVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Request")
                this.AsDynamic().ComplyWithRequest(GraphVm.Request)
                    // This call is needed so there's no warning
                    .ConfigureAwait(false);
        }

        #region Node
        public async Task ComplyWithRequest(AddNode request)
        {
            var cmd = request;
            cmd.Id = Guid.NewGuid();
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
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
                return;
            }
            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(MoveNode request)
        {
            var cmd = request;
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(UpdateNode request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
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
                return;
            }
            ApplyToMap(cmd);
        }
        #endregion

        #region Fiber
        public async Task ComplyWithRequest(AddFiber request)
        {
            var cmd = request;
            cmd.Id = Guid.NewGuid();
            await Bus.SendCommand(cmd);
            ApplyToMap(request);
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
                return;
            }
            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(RequestUpdateFiber request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(RemoveFiber request)
        {
            var cmd = request;
            await Bus.SendCommand(cmd);
            ApplyToMap(request);
        }
        #endregion

        #region Rtu
        public async Task ComplyWithRequest(AddRtuAtGpsLocation request)
        {
            var cmd = request;
            cmd.Id = Guid.NewGuid();
            cmd.NodeId = Guid.NewGuid();
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
        }
        public async Task ComplyWithRequest(RequestUpdateRtu request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(RemoveRtu request)
        {
            var cmd = request;
            if (cmd == null)
                return;
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
        }
        #endregion

        #region Equipment
        public async Task ComplyWithRequest(AddEquipmentAtGpsLocation request)
        {
            var cmd = request;
            cmd.Id = Guid.NewGuid();
            cmd.NodeId = Guid.NewGuid();
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(RequestAddEquipmentIntoNode request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
        }
        public async Task ComplyWithRequest(UpdateEquipment request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(RemoveEquipment request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);

        }
        #endregion

        #region Trace
        public async Task ComplyWithRequest(RequestAddTrace request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            var message = await Bus.SendCommand(cmd);
            if (message != null)
            {
                _windowManager.ShowDialog(new NotificationViewModel(Resources.SID_Error, message));
                return;
            }
            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(AttachTrace request)
        {
            var cmd = request;
            var message = await Bus.SendCommand(cmd);
            if (message != null)
            {
                _windowManager.ShowDialog(new NotificationViewModel(Resources.SID_Error, message));
//                return;
            }
            //            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(DetachTrace request)
        {
            var cmd = request;
            var message = await Bus.SendCommand(cmd);
            if (message != null)
            {
                _windowManager.ShowDialog(new NotificationViewModel(Resources.SID_Error, message));
//                return;
            }
            //            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(RequestAssignBaseRef request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            var message = await Bus.SendCommand(cmd);
            if (message != null)
            {
                _windowManager.ShowDialog(new NotificationViewModel(Resources.SID_Error, message));
                return;
            }
            ApplyToMap(cmd);
        }
        #endregion
    }
}