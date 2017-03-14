using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using PrivateReflectionUsingDynamic;
using Serilog;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel : Screen, IShell
    {
        public ILogger Log { get; set; }

        public Bus Bus { get; }
        private readonly IWindowManager _windowManager;

        public ReadModel ReadModel { get; }
        public TreeOfRtuViewModel TreeOfRtuViewModel { get; set; }
        public GraphReadModel GraphReadModel { get; set; }

        public Db LocalDb { get; set; }
        public ShellViewModel(ReadModel readModel, TreeOfRtuModel treeOfRtuModel, Bus bus, 
            Db db, GraphReadModel graphReadModel, IWindowManager windowManager, ILogger clientLogger)
        {
            ReadModel = readModel;
            TreeOfRtuViewModel = new TreeOfRtuViewModel(treeOfRtuModel);
            Bus = bus;
            LocalDb = db;
            GraphReadModel = graphReadModel;
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
        public async Task ComplyWithRequest(AddRtuAtGpsLocation request)
        {
            var cmd = request;
            cmd.Id = Guid.NewGuid();
            cmd.NodeId = Guid.NewGuid();
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
        public async Task ComplyWithRequest(AddEquipmentAtGpsLocation request)
        {
            var cmd = request;
            cmd.Id = Guid.NewGuid();
            cmd.NodeId = Guid.NewGuid();
            await Bus.SendCommand(cmd);
        }

        public async Task ComplyWithRequest(RequestAddEquipmentIntoNode request)
        {
            await VerboseTasks.AddEquipmentIntoNodeFullTask(request, ReadModel, _windowManager, Bus);
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
}