using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel : Screen, IShell
    {
        private readonly IWindowManager _windowManager;
        public GraphVm GraphVm { get; set; } = new GraphVm();
        public ReadModel ReadModel { get; }
        public Bus Bus { get; }

        public ShellViewModel(IWindowManager windowManager, ReadModel readModel, Bus bus)
        {
            ReadModel = readModel;
            Bus = bus;
            _windowManager = windowManager;
        }

        public void AddOneNode()
        {
            var rtu = new NodeVm()
            {
                Id = Guid.NewGuid(),
                Title = "ÐÒÓ â Ãîìåëå",
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
                NodeA = rtu,
                NodeB = vertSleeve,
                State = FiberState.Critical
            });
            GraphVm.Fibers.Add(new FiberVm()
            {
                Id = Guid.NewGuid(),
                NodeA = rtu,
                NodeB = horizSleeve,
                State = FiberState.Ok
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

        public async Task ComplyWithRequest(AddNode request)
        {
            var cmd = request;
            cmd.Id = Guid.NewGuid();
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(AddNodeIntoFiber request)
        {
            var cmd = request;
            cmd.Id = Guid.NewGuid();
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(MoveNode request)
        {
            var cmd = request;
            await Bus.SendCommand(cmd);
            ApplyToMap(request);
        }

        public async Task ComplyWithRequest(RemoveNode request)
        {
            var cmd = request;
            await Bus.SendCommand(cmd);
            ApplyToMap(request);
        }

        public async Task ComplyWithRequest(AddFiber request)
        {
            var cmd = request;
            await Bus.SendCommand(cmd);
            ApplyToMap(request);
        }

        public async Task ComplyWithRequest(AskAddFiberWithNodes request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            var message = await Bus.SendCommand(cmd);
            if (message != null)
            {
                _windowManager.ShowDialog(new NotificationViewModel("Îøèáêà!", message));
                return;
            }
            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(UpdateFiber request)
        {
            var cmd = request;
            await Bus.SendCommand(cmd);
            ApplyToMap(request);
        }

        public async Task ComplyWithRequest(RemoveFiber request)
        {
            var cmd = request;
            await Bus.SendCommand(cmd);
            ApplyToMap(request);
        }

        public async Task ComplyWithRequest(AskUpdateRtu request)
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

        public async Task ComplyWithRequest(AddRtuAtGpsLocation request)
        {
            var cmd = request;
            cmd.Id = Guid.NewGuid();
            cmd.NodeId = Guid.NewGuid();
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(AddEquipmentAtGpsLocation request)
        {
            var cmd = request;
            cmd.Id = Guid.NewGuid();
            cmd.NodeId = Guid.NewGuid();
            await Bus.SendCommand(cmd);
            ApplyToMap(cmd);
        }

        public async Task ComplyWithRequest(AskAddTrace request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            var message = await Bus.SendCommand(cmd);
            if (message != null)
            {
                _windowManager.ShowDialog(new NotificationViewModel("Îøèáêà!", message));
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
                _windowManager.ShowDialog(new NotificationViewModel("Îøèáêà!", message));
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
                _windowManager.ShowDialog(new NotificationViewModel("Îøèáêà!", message));
//                return;
            }
            //            ApplyToMap(cmd);
        }

        private void ApplyToMap(RemoveRtu cmd)
        {
            var rtuVm = GraphVm.Rtus.First(r => r.Id == cmd.Id);
            var nodeVm = rtuVm.Node;
            GraphVm.Rtus.Remove(rtuVm);
            GraphVm.Nodes.Remove(nodeVm);
        }

        private void ApplyToMap(AddEquipmentAtGpsLocation cmd)
        {
            var nodeVm = new NodeVm()
            {
                Id = cmd.NodeId,
                State = FiberState.Ok,
                Type = cmd.Type,
                Position = new PointLatLng(cmd.Latitude, cmd.Longitude)
            };
            GraphVm.Nodes.Add(nodeVm);

            GraphVm.Equipments.Add(new EquipmentVm() { Id = cmd.Id, Node = nodeVm, Type = cmd.Type });
        }

        private AddTrace PrepareCommand(AskAddTrace ask)
        {
            List<Guid> traceNodes;
            List<Guid> traceEquipments;
            if (!ReadModel.DefineTrace(_windowManager, ask.NodeWithRtuId, ask.LastNodeId,
                out traceNodes, out traceEquipments))
                return null;
            var traceAddViewModel = new TraceAddViewModel(_windowManager, ReadModel, traceNodes, traceEquipments);
            _windowManager.ShowDialog(traceAddViewModel);

            if (!traceAddViewModel.IsUserClickedSave)
                return null;

            return new AddTrace()
            {
                Id = Guid.NewGuid(),
                RtuId = ReadModel.Rtus.First(r => r.NodeId == ask.NodeWithRtuId).Id,
                Title = traceAddViewModel.Title,
                Nodes = traceNodes,
                Equipments = traceEquipments,
                Comment = traceAddViewModel.Comment
            };
        }

        private void ApplyToMap(AddTrace cmd)
        {
            GraphVm.Traces.Add(new TraceVm() { Id = cmd.Id, Nodes = cmd.Nodes });
        }
    }
}