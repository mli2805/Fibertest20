using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    // Could be done for TraceLeaf and PortLeaf
    public class CommonActions
    {
        private readonly IniFile _iniFile35;
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly ClientMeasurementViewModel _clientMeasurementViewModel;
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;

        public CommonActions(IniFile iniFile35, IMyLog logFile, CurrentUser currentUser,
            ClientMeasurementViewModel clientMeasurementViewModel, ReadModel readModel,
            IWindowManager windowManager)
        {
            _iniFile35 = iniFile35;
            _logFile = logFile;
            _currentUser = currentUser;
            _clientMeasurementViewModel = clientMeasurementViewModel;
            _readModel = readModel;
            _windowManager = windowManager;
        }

        public void MeasurementClientAction(object param)
        {
            var parent = GetParent(param);
            if (parent == null) return;

            if (_clientMeasurementViewModel.Initialize(parent, GetPortNumber(param)))
                _windowManager.ShowDialogWithAssignedOwner(_clientMeasurementViewModel);
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
            if (!ToggleToPort(parent, portNumber)) return;

            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuLeaf.Id);
            if (rtu == null) return;

            var mainCharonAddress = rtu.MainChannel;
            mainCharonAddress.Port = 23;
            var mainCharon = new Charon(mainCharonAddress, _iniFile35, _logFile) { OwnPortCount = rtuLeaf.OwnPortCount };

            var addressOfCharonWithThisPort = ((IPortOwner)parent).OtauNetAddress.Ip4Address == @"192.168.88.101" ? mainCharonAddress : ((IPortOwner)parent).OtauNetAddress;
            var otdrPort = addressOfCharonWithThisPort.Port == 23 ? 1500 : addressOfCharonWithThisPort.Port;
            System.Diagnostics.Process.Start(@"..\RftsReflect\Reflect.exe",
                $"-fnw -n {addressOfCharonWithThisPort.Ip4Address} -p {otdrPort}");
        }

        private bool ToggleToPort(Leaf parent, int portNumber)
        {
            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuLeaf.Id);
            if (rtu == null) return false;

            var mainCharonAddress = rtu.MainChannel;
            mainCharonAddress.Port = 23;
            var mainCharon = new Charon(mainCharonAddress, _iniFile35, _logFile) { OwnPortCount = rtuLeaf.OwnPortCount };

            var addressOfCharonWithThisPort = ((IPortOwner)parent).OtauNetAddress.Ip4Address == @"192.168.88.101" ? mainCharonAddress : ((IPortOwner)parent).OtauNetAddress;

            var result = mainCharon.SetExtendedActivePort(addressOfCharonWithThisPort, portNumber);
            if (result == CharonOperationResult.Ok)
                return true;
            var vm = new MyMessageBoxViewModel(MessageType.Error, $@"{mainCharon.LastErrorMessage}");
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
            return rtuLeaf.IsAvailable;
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