using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class GrmRtuRequests
    {
        private readonly ILifetimeScope _globalScope;
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public GrmRtuRequests(ILifetimeScope globalScope, ReadModel readModel, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
        }

        public async Task AddRtuAtGpsLocation(RequestAddRtuAtGpsLocation request)
        {
            var cmd = new AddRtuAtGpsLocation
            {
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Id = Guid.NewGuid(),
                NodeId = Guid.NewGuid()
            };
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public void UpdateRtu(RequestUpdateRtu request)
        {
            var rtu = _readModel.Rtus.First(r => r.NodeId == request.NodeId);
            var vm = _globalScope.Resolve<RtuUpdateViewModel>();
            vm.Initilize(rtu.Id);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async Task RemoveRtu(RequestRemoveRtu request)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.NodeId == request.NodeId);
            if (rtu == null)
                return;
            var cmd = new RemoveRtu() { Id = rtu.Id };
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }
    }
}