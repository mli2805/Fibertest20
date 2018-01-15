using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    // Could be done for TraceLeaf and PortLeaf
    public class CommonActions
    {
        private readonly IniFile _iniFile35;
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly IWindowManager _windowManager;

        public CommonActions(IniFile iniFile35, IMyLog logFile, CurrentUser currentUser,
            IWindowManager windowManager)
        {
            _iniFile35 = iniFile35;
            _logFile = logFile;
            _currentUser = currentUser;
            _windowManager = windowManager;
        }

        public void MeasurementClientAction(object param)
        {
            var parent = GetParent(param);
            if (parent != null)
                DoMeasurementClient(parent, GetPortNumber(param));
        }

        private async void DoMeasurementClient(Leaf parent, int portNumber)
        {
            using (new WaitCursor())
            {
                if (!ToggleToPort(parent, portNumber)) return;
                RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
                var mainCharonAddress = rtuLeaf.OtauNetAddress;

                // TODO ask user measurement parameters, start measurement
                var otdrManager = new OtdrManager(@"..\OtdrMeasEngine\", _iniFile35, _logFile);
                var initializationResult = otdrManager.LoadDll();
                if (initializationResult != "")
                {
                    var message = new List<string> { Resources.SID_OTDR_initialization_error_, "", initializationResult };
                    var errorVm = new MyMessageBoxViewModel(MessageType.Error, message, 2);
                    _windowManager.ShowDialog(errorVm);
                    return;
                }

                if (!otdrManager.InitializeLibrary())
                    return;

                await Task.Run(() => otdrManager.ConnectOtdr(mainCharonAddress.Ip4Address));
                if (!otdrManager.IsOtdrConnected)
                    return;
                var vm = new OtdrParametersSetterViewModel(otdrManager.IitOtdr);
                IWindowManager windowManager = new WindowManager();
                windowManager.ShowDialog(vm);


            }
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

            var addressOfCharonWithThisPort = ((IPortOwner)parent).OtauNetAddress;
            var otdrPort = addressOfCharonWithThisPort.Port == 23 ? 1500 : addressOfCharonWithThisPort.Port;
            System.Diagnostics.Process.Start(@"..\RftsReflect\Reflect.exe",
                $"-fnw -n {addressOfCharonWithThisPort.Ip4Address} -p {otdrPort}");
        }

        private bool ToggleToPort(Leaf parent, int portNumber)
        {
            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            var mainCharonAddress = rtuLeaf.OtauNetAddress;
            var mainCharon = new Charon(mainCharonAddress, _iniFile35, _logFile) { OwnPortCount = rtuLeaf.OwnPortCount };

            var addressOfCharonWithThisPort = ((IPortOwner)parent).OtauNetAddress;
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