using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class GrmFiberWithNodesRequest
    {
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly Model _model;
        private readonly IWindowManager _windowManager;

        public GrmFiberWithNodesRequest(IWcfServiceForClient c2DWcfManager, Model model, IWindowManager windowManager)
        {
            _c2DWcfManager = c2DWcfManager;
            _model = model;
            _windowManager = windowManager;
        }

        public async Task AddFiberWithNodes(RequestAddFiberWithNodes request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            var message =
                await _c2DWcfManager.SendCommandAsObj(cmd);
            if (message != null)
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, message));
            }
        }

        private AddFiberWithNodes PrepareCommand(RequestAddFiberWithNodes request)
        {
            if (!Validate(request))
                return null;

            var vm = new FiberWithNodesAddViewModel();
            _windowManager.ShowDialogWithAssignedOwner(vm);
            if (!vm.Result)
                return null;

            return EndFiberCreationMany(request, vm.Count, vm.GetSelectedType());
        }

        private bool Validate(RequestAddFiberWithNodes request)
        {
            if (request.Node1 == request.Node2)
                return false;
            var fiber =
                _model.Fibers.FirstOrDefault(f =>
                        f.NodeId1 == request.Node1 && f.NodeId2 == request.Node2 ||
                        f.NodeId1 == request.Node2 && f.NodeId2 == request.Node1);
            if (fiber == null)
                return true;
            _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Section_already_exists));
            return false;
        }

        private AddFiberWithNodes EndFiberCreationMany(RequestAddFiberWithNodes request, int count, EquipmentType type)
        {
            var result = new AddFiberWithNodes()
            {
                Node1 = request.Node1,
                Node2 = request.Node2,
                AddEquipments = new List<AddEquipmentAtGpsLocation>()
            };
            List<Guid> nodeIds = new List<Guid>();

            var intermediateNodes = CreateIntermediateNodes(request.Node1, request.Node2, count, type);
            foreach (var cmd in intermediateNodes)
            {
                result.AddEquipments.Add(cmd);
                nodeIds.Add(cmd.NodeId);
            }

            nodeIds.Insert(0, request.Node1);
            nodeIds.Add(request.Node2);
            result.AddFibers = CreateIntermediateFibers(nodeIds, count).ToList();
            return result;
        }

        private IEnumerable<AddEquipmentAtGpsLocation> CreateIntermediateNodes(Guid startId, Guid finishId, int count, EquipmentType type)
        {
            var startNode = _model.Nodes.First(n => n.NodeId == startId);
            var finishNode = _model.Nodes.First(n => n.NodeId == finishId);

            double deltaLat = (finishNode.Position.Lat - startNode.Position.Lat) / (count + 1);
            double deltaLng = (finishNode.Position.Lng - startNode.Position.Lng) / (count + 1);

            for (int i = 0; i < count; i++)
            {
                double lat = startNode.Position.Lat + deltaLat * (i + 1);
                double lng = startNode.Position.Lng + deltaLng * (i + 1);

                var cmd = new AddEquipmentAtGpsLocation()
                {
                    RequestedEquipmentId = Guid.NewGuid(),
                    NodeId = Guid.NewGuid(),
                    Latitude = lat,
                    Longitude = lng,
                    Type = type
                };
                cmd.EmptyNodeEquipmentId = type <= EquipmentType.EmptyNode ? Guid.Empty : Guid.NewGuid();
                yield return cmd;
            }
        }

        private IEnumerable<AddFiber> CreateIntermediateFibers(List<Guid> nodes, int count)
        {
            for (int i = 0; i <= count; i++)
                yield return new AddFiber() { FiberId = Guid.NewGuid(), NodeId1 = nodes[i], NodeId2 = nodes[i + 1] };
        }
    }
}