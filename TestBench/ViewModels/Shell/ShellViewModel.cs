using System;
using System.Collections.Generic;
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

        public ShellViewModel(ReadModel readModel, TreeReadModel treeReadModel, Bus bus, IWindowManager windowManager, ILogger clientLogger)
        {
            ReadModel = readModel;
            MyLeftPanelViewModel = new LeftPanelViewModel(treeReadModel);
            Bus = bus;
            _windowManager = windowManager;

            Log = clientLogger;
            Log.Information(@"Client started!");

            PopulateWithTraceWithBase(bus);
        }

        private void PopulateWithTraceWithBase(Bus bus)
        {
            var rtuNodeId = Guid.NewGuid();
            var rtuId = Guid.NewGuid();
            Bus.SendCommand(new AddRtuAtGpsLocation()
            {
                Id = rtuId,
                NodeId = rtuNodeId,
                Latitude = 52.429333,
                Longitude = 31.006683
            });

            Bus.SendCommand(new UpdateRtu()
            {
                Id = rtuId,
                Title = @"Grushauka 203",
            });

            var middleNodeId = Guid.NewGuid();
            Bus.SendCommand(new AddNode()
            {
                Id = middleNodeId,
                Latitude = 52.329333,
                Longitude = 30.906683
            });

            bus.SendCommand(new AddFiber()
            {
                Id = Guid.NewGuid(),
                Node1 = rtuNodeId,
                Node2 = middleNodeId
            });

            var lastNodeId = Guid.NewGuid();
            var terminalId = Guid.NewGuid();
            Bus.SendCommand(new AddEquipmentAtGpsLocation()
            {
                Id = terminalId,
                NodeId = lastNodeId,
                Type = EquipmentType.Terminal,
                Latitude = 52.229333,
                Longitude = 30.806683
            });

            bus.SendCommand(new AddFiber()
            {
                Id = Guid.NewGuid(),
                Node1 = middleNodeId,
                Node2 = lastNodeId
            });

            var traceId = Guid.NewGuid();
            bus.SendCommand(new AddTrace()
            {
                Id = traceId,
                RtuId = rtuId,
                Nodes = new List<Guid>() {rtuNodeId, middleNodeId, lastNodeId},
                Equipments = new List<Guid>() {rtuId, Guid.Empty, terminalId},
                Title = @"Trace from Grushauka to Kolodzishschi",
                Comment = @"Trace was built from code",
            });

            var ids = new Dictionary<BaseRefType, Guid>();
            var preciseId = Guid.NewGuid();
            ids.Add(BaseRefType.Precise, preciseId);
            var fastId = Guid.NewGuid();
            ids.Add(BaseRefType.Fast, fastId);

            var contents = new Dictionary<Guid, byte[]>();
            contents.Add(preciseId, new byte[10000]);
            contents.Add(fastId, new byte[10000]);
            bus.SendCommand(new AssignBaseRef()
            {
                TraceId = traceId,
                Ids = ids,
                Contents = contents,
            });
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