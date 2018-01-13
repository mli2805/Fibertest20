using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    // Could be done for TraceLeaf and PortLeaf
    public class CommonActions
    {
        private readonly IniFile _iniFile35;
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;

        public CommonActions(IniFile iniFile35, IMyLog logFile, CurrentUser currentUser,
            ReadModel readModel, IWindowManager windowManager)
        {
            _iniFile35 = iniFile35;
            _logFile = logFile;
            _currentUser = currentUser;
            _readModel = readModel;
            _windowManager = windowManager;
        }

        public void MeasurementClientAction(object param)
        {
            var parent = GetParent(param);
            if (parent != null)
                DoMeasurementClient(parent, GetPortNumber(param));
        }

        private void DoMeasurementClient(Leaf parent, int portNumber)
        {
            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            var otdrAddress = _readModel.Rtus.First(r => r.Id == rtuLeaf.Id).OtdrNetAddress;
            NetAddress otauAddress = new NetAddress(otdrAddress.Ip4Address, 23);

            ToggleToSelectedPort(otauAddress, portNumber);

            // TODO ask user measurement parameters, start measurement
        }

        public void MeasurementRftsReflectAction(object param)
        {
            var parent = GetParent(param);
            if (parent != null)
                DoMeasurementRftsReflection(parent, GetPortNumber(param));
        }

        private Leaf GetParent(object param)
        {
            if (param is Leaf leaf)
                return leaf.Parent;
            return null;
        }

        private int GetPortNumber(object param)
        {
            if (param is IPortNumber leaf)
                return leaf.PortNumber;
            return 0;
        }

        private void DoMeasurementRftsReflection(Leaf parent, int portNumber)
        {
            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            var mainCharonAddress = rtuLeaf.OtauNetAddress;
            var mainCharon = new Charon(mainCharonAddress, _iniFile35, _logFile);
            mainCharon.OwnPortCount = rtuLeaf.OwnPortCount;

            var portOwner = (IPortOwner)parent;
            var charonWithThisPortAddresss = portOwner.OtauNetAddress;

            var result = mainCharon.SetExtendedActivePort(charonWithThisPortAddresss, portNumber);
            if (result != CharonOperationResult.Ok)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, $@"{mainCharon.LastErrorMessage}");
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            var otdrPort = charonWithThisPortAddresss.Port == 23 ? 1500 : charonWithThisPortAddresss.Port;
            System.Diagnostics.Process.Start(@"..\RftsReflect\Reflect.exe",
                $"-fnw -n {charonWithThisPortAddresss.Ip4Address} -p {otdrPort}");
        }

        private bool ToggleToSelectedPort(NetAddress otauAddress, int portNumber)
        {
            var charon = new Charon(otauAddress, _iniFile35, _logFile);
            var result = charon.SetExtendedActivePort(charon.NetAddress, portNumber);
            if (result == CharonOperationResult.Ok)
                return true;

            var vm = new MyMessageBoxViewModel(MessageType.Error, $@"{charon.LastErrorMessage}");
            _windowManager.ShowDialogWithAssignedOwner(vm);
            return false;
        }


        public bool CanMeasurementClientAction(object param)
        {
            if (_currentUser.Role >= Role.Operator)
                return false;

            var parent = GetParent(param);
            if (parent == null)
                return false;

            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            return rtuLeaf.IsAvailable && rtuLeaf.MonitoringState == MonitoringState.Off;
        }

        public bool CanMeasurementRftsReflectAction(object param)
        {
            if (_currentUser.Role >= Role.Operator)
                return false;

            var parent = GetParent(param);
            if (parent == null)
                return false;

            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            return rtuLeaf.IsAvailable && rtuLeaf.MonitoringState == MonitoringState.Off;
        }

    }
}