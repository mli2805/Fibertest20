using System;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class GrmEquipmentRequests
    {
        private readonly ILifetimeScope _globalScope;
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public GrmEquipmentRequests(ILifetimeScope globalScope, ReadModel readModel, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
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
            await VerboseTasks.AddEquipmentIntoNodeFullTask(request, _globalScope, _readModel, _windowManager, _c2DWcfManager);
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
