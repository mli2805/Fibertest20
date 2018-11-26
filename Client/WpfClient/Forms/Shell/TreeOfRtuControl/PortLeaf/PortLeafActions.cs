using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class PortLeafActions
    {
        private readonly OtauToAttachViewModel _otauToAttachViewModel;
        private readonly TraceToAttachViewModel _traceToAttachViewModel;
        private readonly CurrentUser _currentUser;
        private readonly IWindowManager _windowManager;

        public PortLeafActions(CurrentUser currentUser, IWindowManager windowManager,
            OtauToAttachViewModel otauToAttachViewModel, TraceToAttachViewModel traceToAttachViewModel)
        {
            _currentUser = currentUser;
            _windowManager = windowManager;
            _otauToAttachViewModel = otauToAttachViewModel;
            _traceToAttachViewModel = traceToAttachViewModel;
        }

        public void AttachTraceFromListAction(object param)
        {
            if (!(param is PortLeaf portLeaf))
                return;

            var rtuId = portLeaf.Parent is RtuLeaf ? portLeaf.Parent.Id : portLeaf.Parent.Parent.Id;
            var otauPortDto = new OtauPortDto()
            {
//                OtauIp = ((IPortOwner)portLeaf.Parent).OtauNetAddress.Ip4Address,
//                OtauTcpPort = ((IPortOwner)portLeaf.Parent).OtauNetAddress.Port,
                Serial = ((IPortOwner)portLeaf.Parent).Serial,
                IsPortOnMainCharon = portLeaf.Parent is RtuLeaf,
                OpticalPort = portLeaf.PortNumber
            };
            _traceToAttachViewModel.Initialize(rtuId, otauPortDto);
            _windowManager.ShowDialogWithAssignedOwner(_traceToAttachViewModel);
        }

        public void AttachOtauAction(object param)
        {
            if (!(param is PortLeaf portLeaf))
                return;

            _otauToAttachViewModel.Initialize(portLeaf.Parent.Id, portLeaf.PortNumber);
            _windowManager.ShowDialogWithAssignedOwner(_otauToAttachViewModel);
        }

        public bool CanAttachOtauAction(object param)
        {
            if (_currentUser.Role > Role.Root)
                return false;
            if (!(param is PortLeaf portLeaf))
                return false;

            if (portLeaf.Parent is OtauLeaf)
                return false;
            var rtuLeaf = (RtuLeaf)portLeaf.Parent;
            return rtuLeaf.IsAvailable && rtuLeaf.MonitoringState == MonitoringState.Off ;
        }

        public bool CanAttachTraceAction(object param)
        {
            if (_currentUser.Role > Role.Operator)
                return false;
            if (!(param is PortLeaf portLeaf))
                return false;

            var rtuLeaf = portLeaf.Parent is RtuLeaf leaf ? leaf : (RtuLeaf)portLeaf.Parent.Parent;

            var hasFreeTraces = rtuLeaf.ChildrenImpresario.Children.Any(c=>c is TraceLeaf && ((TraceLeaf)c).PortNumber < 1);
            return rtuLeaf.IsAvailable && hasFreeTraces;
        }
    }
}