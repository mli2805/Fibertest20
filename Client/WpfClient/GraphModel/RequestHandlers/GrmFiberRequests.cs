using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class GrmFiberRequests
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;


        public GrmFiberRequests(ILifetimeScope globalScope, IWcfServiceForClient c2DWcfManager, ReadModel readModel, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
            _windowManager = windowManager;
        }

        public async Task AddFiber(AddFiber cmd)
        {
            if (!Validate(cmd)) return;
            cmd.FiberId = Guid.NewGuid();
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        private bool Validate(AddFiber cmd)
        {
            if (cmd.NodeId1 == cmd.NodeId2)
                return false;
            var fiber =
                _readModel.Fibers.FirstOrDefault(f =>
                        f.NodeId1 == cmd.NodeId1 && f.NodeId2 == cmd.NodeId2 ||
                        f.NodeId1 == cmd.NodeId2 && f.NodeId2 == cmd.NodeId1);
            if (fiber == null)
                return true;
            _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Section_already_exists));
            return false;
        }

        public async Task UpdateFiber(RequestUpdateFiber request)
        {
            var cmd = PrepareCommand(request);
            if (cmd == null)
                return;
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        private UpdateFiber PrepareCommand(RequestUpdateFiber request)
        {
            var vm = _globalScope.Resolve<FiberUpdateViewModel>();
            vm.Initialize(request.Id);
            _windowManager.ShowDialogWithAssignedOwner(vm);

            return vm.Command;
        }

        public async Task RemoveFiber(RemoveFiber cmd)
        {
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }
    }
}