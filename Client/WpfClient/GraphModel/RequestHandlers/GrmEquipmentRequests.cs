using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class GrmEquipmentRequests
    {
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly AddEquipmentIntoNodeBuilder _addEquipmentIntoNodeBuilder;

        public GrmEquipmentRequests(IWcfServiceDesktopC2D c2DWcfManager, AddEquipmentIntoNodeBuilder addEquipmentIntoNodeBuilder)
        {
            _c2DWcfManager = c2DWcfManager;
            _addEquipmentIntoNodeBuilder = addEquipmentIntoNodeBuilder;
        }

        public async Task AddEquipmentAtGpsLocation(RequestAddEquipmentAtGpsLocation request)
        {
            var cmd = new AddEquipmentAtGpsLocation()
            {
                RequestedEquipmentId = Guid.NewGuid(),
                NodeId = Guid.NewGuid(),
                Type = request.Type,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
            };
            cmd.EmptyNodeEquipmentId = request.Type == EquipmentType.EmptyNode || request.Type == EquipmentType.AdjustmentPoint ? Guid.Empty : Guid.NewGuid();
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task AddEquipmentIntoNode(RequestAddEquipmentIntoNode request)
        {
            var cmd = _addEquipmentIntoNodeBuilder.BuildCommand(request.NodeId);
            if (cmd == null)
                return;
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task UpdateEquipment(UpdateEquipment cmd)
        {
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task RemoveEquipment(RemoveEquipment cmd)
        {
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }


    }
}
