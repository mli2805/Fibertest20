using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class PortLeaf : Leaf
    {
        private readonly IniFile _iniFile35;
        private readonly IMyLog _logFile;
        public readonly int PortNumber;
        public readonly int ExtendedPortNumber;

        public override string Name
        {
            get
            {
                    return string.Format(Resources.SID_Port_N, PortNumber);
            }
            set { }
        }
        public int LeftMargin => Parent is OtauLeaf ? 106 : 85;

        public PortLeaf(ReadModel readModel, IMyWindowManager windowManager, IWcfServiceForClient c2DWcfManager, IniFile iniFile35, IMyLog logFile, PostOffice postOffice, Leaf parent, int portNumber)
            : base(readModel, windowManager, c2DWcfManager, postOffice)
        {
            _iniFile35 = iniFile35;
            _logFile = logFile;
            PortNumber = portNumber;
            Parent = parent;
            var otauLeaf = Parent as OtauLeaf;
            ExtendedPortNumber = otauLeaf != null ? otauLeaf.FirstPortNumber + PortNumber - 1 : PortNumber;
            Color = Brushes.Black;
        }

        protected override List<MenuItemVm> GetMenuItems()
        {
            IsSelected = true;

            var menu = new List<MenuItemVm>();
            menu.AddRange(GetFreePortMenuItems());
            menu.AddRange(GetAnyPortMenuItems());
            return menu;
        }
        private IEnumerable<MenuItemVm> GetFreePortMenuItems()
        {
            yield return new MenuItemVm()
            {
                Header = Resources.SID_Attach_from_list,
                Command = new ContextMenuAction(AttachFromListAction, CanAttachTraceAction),
                CommandParameter = this,
            };

            yield return new MenuItemVm()
            {
                Header = Resources.SID_Attach_optical_switch,
                Command = new ContextMenuAction(AttachOtauAction, CanAttachOtauAction),
                CommandParameter = this,
            };
        }

        private IEnumerable<MenuItemVm> GetAnyPortMenuItems()
        {
            yield return null;

            yield return new MenuItemVm()
            {
                Header = Resources.SID_Measurement__Client_,
                Command = new ContextMenuAction(new PortExtensions().MeasurementClientAction, CanSomeAction),
                CommandParameter = this,
            };

            yield return new MenuItemVm()
            {
                Header = Resources.SID_Measurement__RFTS_Reflect_,
                Command = new ContextMenuAction(MeasurementRftsReflectAction, CanSomeAction),
                CommandParameter = this,
            };
        }

        public void AttachFromListAction(object param)
        {
            var rtuId = Parent is RtuLeaf ? Parent.Id : Parent.Parent.Id;
            var otauPortDto = new OtauPortDto()
            {
                OtauIp = ((IPortOwner)Parent).OtauNetAddress.Ip4Address,
                OtauTcpPort = ((IPortOwner)Parent).OtauNetAddress.Port,
                IsPortOnMainCharon = Parent is RtuLeaf,
                OpticalPort = PortNumber
            };
            var vm = new TraceToAttachViewModel(rtuId, ExtendedPortNumber, otauPortDto, ReadModel, C2DWcfManager, WindowManager);
            WindowManager.ShowDialog(vm);
        }

        public void AttachOtauAction(object param)
        {
            var vm = new OtauToAttachViewModel(Parent.Id, PortNumber, ReadModel, C2DWcfManager, WindowManager, _iniFile35, _logFile);
            WindowManager.ShowDialog(vm);
        }

        private bool CanAttachOtauAction(object param)
        {
            if (Parent is OtauLeaf)
                return false;
            var rtuLeaf = (RtuLeaf)Parent;
            return rtuLeaf.IsAvailable && !rtuLeaf.HasAttachedTraces;
        }

        private bool CanAttachTraceAction(object param)
        {
            var rtuLeaf = Parent is RtuLeaf ? (RtuLeaf) Parent : (RtuLeaf) Parent.Parent;
            return rtuLeaf.IsAvailable;
        }

        private void MeasurementRftsReflectAction(object param)
        {
            RtuLeaf rtuLeaf = Parent is RtuLeaf ? (RtuLeaf)Parent : (RtuLeaf)Parent.Parent;
            var otdrAddress = ReadModel.Rtus.First(r => r.Id == rtuLeaf.Id).OtdrNetAddress;
            NetAddress otauAddress = new NetAddress(otdrAddress.Ip4Address, 23);

            var charon = new Charon(otauAddress, _iniFile35, _logFile);
            var result = charon.SetExtendedActivePort(charon.NetAddress, ExtendedPortNumber);
            if (result == CharonOperationResult.Ok)
                System.Diagnostics.Process.Start(@"TraceEngine\Reflect.exe", $"-fnw -n {otdrAddress.Ip4Address} -p {otdrAddress.Port}");
            else
            {
                var vm = new NotificationViewModel(Resources.SID_Error, $@"{charon.LastErrorMessage}");
                WindowManager.ShowDialog(vm);
            }
        }
    }
}