using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
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
            Log.Information("Client started!");
        }

        public void AddOneNode()
        {
            var rtu = new NodeVm()
            {
                Id = Guid.NewGuid(),
                Title = "РТУ в Гомеле",
                State = FiberState.Ok,
                Type = EquipmentType.Rtu,
                Position = new PointLatLng(52.429333, 31.006683)
            };
            GraphVm.Nodes.Add(rtu);
        }

        public void Populate()
        {
            var rtu = new NodeVm()
            {
                Id = Guid.NewGuid(),
                Title = "first rtu",
                State = FiberState.Ok,
                Type = EquipmentType.Rtu,
                Position = new PointLatLng(55.088345, 25.019362)
            };
            GraphVm.Nodes.Add(rtu);
            var vertSleeve = new NodeVm()
            {
                Id = Guid.NewGuid(),
                Title = "vert sleeve",
                State = FiberState.Critical,
                Type = EquipmentType.Sleeve,
                Position = new PointLatLng(52.301848, 27.018362)
            };
            GraphVm.Nodes.Add(vertSleeve);
            var horizSleeve = new NodeVm()
            {
                Id = Guid.NewGuid(),
                Title = "horiz sleeve",
                State = FiberState.Ok,
                Type = EquipmentType.Sleeve,
                Position = new PointLatLng(53.287345, 31.016841)
            };
            GraphVm.Nodes.Add(horizSleeve);

            GraphVm.Fibers.Add(new FiberVm()
            {
                Id = Guid.NewGuid(),
                Node1 = rtu,
                Node2 = vertSleeve,
                State = FiberState.Critical
            });
            GraphVm.Fibers.Add(new FiberVm()
            {
                Id = Guid.NewGuid(),
                Node1 = rtu,
                Node2 = horizSleeve,
                State = FiberState.Ok
            });
        }

        public void AssignBase()
        {
            var traceId = GraphVm.Traces.First().Id;
            var vm = new BaseRefsAssignViewModel(traceId, ReadModel);
            _windowManager.ShowDialog(vm);

            if (vm.Command != null)
                Bus.SendCommand(vm.Command).Wait();
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
                _windowManager.ShowDialog(new NotificationViewModel("Ошибка!", message));
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
                _windowManager.ShowDialog(new NotificationViewModel("Ошибка!", message));
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
                _windowManager.ShowDialog(new NotificationViewModel("Ошибка!", message));
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
                _windowManager.ShowDialog(new NotificationViewModel("Ошибка!", message));
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
                _windowManager.ShowDialog(new NotificationViewModel("Ошибка!", message));
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
                _windowManager.ShowDialog(new NotificationViewModel("Ошибка!", message));
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
                _windowManager.ShowDialog(new NotificationViewModel("Ошибка!", message));
                return;
            }
            ApplyToMap(cmd);
        }
        #endregion
    }
}