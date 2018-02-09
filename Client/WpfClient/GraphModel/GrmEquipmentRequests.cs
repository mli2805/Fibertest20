using System;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class GrmEquipmentRequests
    {
        private readonly IWcfServiceForClient _c2DWcfManager;

        public GrmEquipmentRequests(IWcfServiceForClient c2DWcfManager)
        {
            _c2DWcfManager = c2DWcfManager;
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

      
    }
}
