using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel : Screen, IShell
    {
        private readonly IWindowManager _windowManager;
        public GraphVm GraphVm { get; set; } = new GraphVm();
        public ReadModel ReadModel { get; set; } = new ReadModel();
        public Aggregate Aggregate { get; set; } = new Aggregate();
        public ClientPoller ClientPoller { get; set; }

        public ShellViewModel(IWindowManager windowManager)
        {
            if (windowManager == null)
                _windowManager = new WindowManager();
            else
                _windowManager = windowManager;
            ClientPoller = new ClientPoller(Aggregate.WriteModel.Db, new List<object> { ReadModel });
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
            if (e.PropertyName == "Ask")
                ProcessAsk(GraphVm.Ask);
        }
        public void ProcessAsk(object ask)
        {
            if (ask is AddMarker)
                ApplyToMap((AddMarker)ask);

            #region Node

            if (ask is AddNode)
            {
                var cmd = (AddNode)ask;
                cmd.Id = Guid.NewGuid();
                Aggregate.When(cmd);
                ClientPoller.Tick();
                ApplyToMap(cmd);
            }
            if (ask is AddNodeIntoFiber)
            {
                // validation if fiber used by trace with base 
                // was applied inside context menu

                // here should be many requests to user - 
                // type of node visible or not, add equipment or not, 
                // which traces if there any should use equipment if it was added
                // after this we have fully filled in AddFiberWithNodes command

                // next we should send this command to Aggregate and
                // if Aggregate executed it new events will appeare
                // and ReadModel will apply them

                // 
                ApplyToMap((AddNodeIntoFiber)ask);
            }
            if (ask is MoveNode)
                ApplyToMap((MoveNode)ask);
            if (ask is RemoveNode)
                ApplyToMap((RemoveNode)ask);

            #endregion

            #region Fiber

            if (ask is AskAddFiberWithNodes)
            {
                var cmd = PrepareCommand((AskAddFiberWithNodes)ask);
                if (cmd == null)
                    return;
                var message = Aggregate.When(cmd);
                if (message != null)
                {
                    _windowManager.ShowDialog(new NotificationViewModel("Ошибка!", message));
                    return;
                }
                ClientPoller.Tick();
                ApplyToMap(cmd);
            }
            if (ask is AddFiber)
            {
                Aggregate.When((AddFiber)ask);
                ClientPoller.Tick();
                ApplyToMap((AddFiber)ask);
            }
            if (ask is UpdateFiber)
                ApplyToMap((UpdateFiber)ask);
            if (ask is RemoveFiber)
                ApplyToMap((RemoveFiber)ask);

            #endregion

            if (ask is AddRtuAtGpsLocation)
            {
                var cmd = (AddRtuAtGpsLocation)ask;
                cmd.Id = Guid.NewGuid();
                cmd.NodeId = Guid.NewGuid();
                Aggregate.When(cmd);
                ClientPoller.Tick();
                ApplyToMap(cmd);
            }
            if (ask is AddEquipmentAtGpsLocation)
            {
                var cmd = (AddEquipmentAtGpsLocation)ask;
                cmd.Id = Guid.NewGuid();
                cmd.NodeId = Guid.NewGuid();
                Aggregate.When(cmd);
                ClientPoller.Tick();
                ApplyToMap(cmd);
            }

            if (ask is AskAddTrace)
            {
                var cmd = PrepareCommand((AskAddTrace)ask);
                if (cmd == null)
                    return;
                var message = Aggregate.When(cmd);
                if (message != null)
                {
                    _windowManager.ShowDialog(new NotificationViewModel("Ошибка!", message));
                    return;
                }
                ClientPoller.Tick();
                ApplyToMap(cmd);
            }

        }



        private void ApplyToMap(AddMarker cmd)
        {
            var markerVm = new MarkerVm() { Id = Guid.NewGuid(), Position = new PointLatLng(cmd.Latitude, cmd.Longitude) };
            GraphVm.MarkerVms.Add(markerVm);
        }

        private void ApplyToMap(AddRtuAtGpsLocation cmd)
        {
            var nodeVm = new NodeVm()
            {
                Id = cmd.NodeId,
                State = FiberState.Ok,
                Type = EquipmentType.Rtu,
                Position = new PointLatLng(cmd.Latitude, cmd.Longitude)
            };
            GraphVm.Nodes.Add(nodeVm);
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
        }

        private AddTrace PrepareCommand(AskAddTrace ask)
        {
            List<Guid> traceNodes;
            List<Guid> traceEquipments;
            if (!ReadModel.DefineTrace(_windowManager, ask.NodeWithRtuId, ask.LastNodeId,
                out traceNodes, out traceEquipments))
                return null;
            var traceAddViewModel = new TraceAddViewModel();
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