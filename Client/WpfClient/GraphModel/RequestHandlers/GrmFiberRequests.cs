using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class GrmFiberRequests
    {
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;


        public GrmFiberRequests(IWcfServiceForClient c2DWcfManager, ReadModel readModel, IWindowManager windowManager)
        {
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
            _windowManager = windowManager;
        }

        public async Task AddFiber(AddFiber cmd)
        {
            if (!Validate(cmd)) return;
            cmd.Id = Guid.NewGuid();
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        private bool Validate(AddFiber cmd)
        {
            if (cmd.Node1 == cmd.Node2)
                return false;
            var fiber =
                _readModel.Fibers.FirstOrDefault(f =>
                        f.Node1 == cmd.Node1 && f.Node2 == cmd.Node2 ||
                        f.Node1 == cmd.Node2 && f.Node2 == cmd.Node1);
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
            var vm = new FiberUpdateViewModel(request.Id, _readModel);
            _windowManager.ShowDialogWithAssignedOwner(vm);

            return vm.Command;
        }

        public async Task RemoveFiber(RemoveFiber cmd)
        {
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }
    }
}