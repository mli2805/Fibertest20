using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class RtuRemover
    {
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;

        public RtuRemover(IWindowManager windowManager, IWcfServiceDesktopC2D c2DWcfManager)
        {
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
        }

        public async Task<string> Fire(Rtu rtu)
        {
            var vm = new MyMessageBoxViewModel(MessageType.Confirmation, string.Format(Resources.SID_Remove_RTU___0____, rtu.Title));
            _windowManager.ShowDialogWithAssignedOwner(vm);
            if (!vm.IsAnswerPositive) return null;
            var cmd = new RemoveRtu() { RtuId = rtu.Id, RtuNodeId = rtu.NodeId };
            return await _c2DWcfManager.SendCommandAsObj(cmd);
        }
    }
}