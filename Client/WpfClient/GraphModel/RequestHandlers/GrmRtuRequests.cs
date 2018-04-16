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
        private readonly Model _model;
        private readonly CurrentUser _currentUser;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public GrmRtuRequests(ILifetimeScope globalScope, Model model, CurrentUser currentUser,
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _model = model;
            _currentUser = currentUser;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
        }

        public void AddRtuAtGpsLocation(RequestAddRtuAtGpsLocation request)
        {
            var vm = _globalScope.Resolve<RtuUpdateViewModel>();
            vm.Initialize(request);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void UpdateRtu(RequestUpdateRtu request)
        {
            var rtu = _model.Rtus.First(r => r.NodeId == request.NodeId);
            var vm = _globalScope.Resolve<RtuUpdateViewModel>();
            vm.Initialize(rtu.Id);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async Task RemoveRtu(RequestRemoveRtu request)
        {
            var rtu = _model.Rtus.First(r => r.NodeId == request.NodeId);
            var cmd = new RemoveRtu() { RtuId = rtu.Id, RtuNodeId = request.NodeId };
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public void DefineTraceStepByStep(Guid rtuNodeId, string rtuTitle)
        {
            var vm = _globalScope.Resolve<TraceStepByStepViewModel>();
            vm.Initialize(rtuNodeId, rtuTitle);
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public async Task SaveUsersHiddenRtus(Guid rtuNodeId)
        {
            var rtu = _model.Rtus.First(r => r.NodeId == rtuNodeId);
            var user = _model.Users.First(u => u.UserId == _currentUser.UserId);
            if (user.HiddenRtus.Contains(rtu.Id))
                user.HiddenRtus.Remove(rtu.Id);
            else
                user.HiddenRtus.Add(rtu.Id);
            var cmd = new SaveUsersHiddentRtus(){UserId = user.UserId, HiddenRtus = user.HiddenRtus};
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }
    }
}