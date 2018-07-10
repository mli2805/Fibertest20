using System.Collections.Generic;
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
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;

        public CommonActions(IniFile iniFile35, IMyLog logFile, CurrentUser currentUser,
            ClientMeasurementViewModel clientMeasurementViewModel, Model readModel,
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


        /*
         * Old fashioned RTU has main address 192.168.96.58 which is different from its otau address 192.168.96.59
         *      mainCharon = 192.168.96.59 : 23    addressOfCharonWithThisPort = 192.168.96.59 : 23
         *          and than otdr address = 192.168.96.59 : 1500
         *
         * New RTU (MAK100) has main address 172.16.5.53 and its otau could be addressed by the same address, but in otau address stored value 192.168.88.101
         *      mainCharon = 172.16.5.53 : 23    addressOfCharonWithThisPort = 172.16.5.53 : 23
         *          and than otdr address = 172.16.5.53 : 1500
         *
         * Every of these RTU could be augmented with BOP (let it be that additional otau has address 172.16.5.57 : 11834)
         * but OTDR is always part of RTU, not of BOP
         *
         *      so Old RTU with BOP:  mainCharon = 192.168.96.59 : 23      addressOfCharonWithThisPort = 172.16.5.57 : 11834
         *          and than otdr address = 192.168.96.59 : 1500
         *
         *      while MAK100 with BOP:   mainCharon = 172.16.5.53 : 23      addressOfCharonWithThisPort = 172.16.5.57 : 11834
         *          and than otdr address = 172.16.5.53 : 1500
         */
        private void DoMeasurementRftsReflection(Leaf parent, int portNumber)
        {
            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            var isMak100 = rtuLeaf.OtauNetAddress.Ip4Address == @"192.168.88.101";
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuLeaf.Id);
            if (rtu == null) return;

            var mainCharonAddress = isMak100 ? rtu.MainChannel : rtu.OtdrNetAddress;
            mainCharonAddress.Port = 23;
            var mainCharon = new Charon(mainCharonAddress, _iniFile35, _logFile) { OwnPortCount = rtuLeaf.OwnPortCount };

            NetAddress addressOfCharonWithThisPort;
            if (parent is OtauLeaf otauLeaf)
            {
                addressOfCharonWithThisPort = otauLeaf.OtauNetAddress;
                var bopCharon = new Charon(addressOfCharonWithThisPort, _iniFile35, _logFile);
                bopCharon.OwnPortCount = otauLeaf.OwnPortCount;
                mainCharon.Children = new Dictionary<int, Charon> { {otauLeaf.MasterPort, bopCharon} };
            }
            else
            {
                addressOfCharonWithThisPort = mainCharonAddress;
            }

            if (!ToggleToPort(mainCharon, addressOfCharonWithThisPort, portNumber)) return;

            var otdrPort = 1500;
           // System.Diagnostics.Process.Start(@"C:\Iit-Fibertest\RftsReflect\Reflect.exe",
            System.Diagnostics.Process.Start(@"..\..\RftsReflect\Reflect.exe",
                $@"-fnw -n {mainCharonAddress.Ip4Address} -p {otdrPort}");
        }

        private bool ToggleToPort(Charon mainCharon, NetAddress addressOfCharonWithThisPort, int portNumber)
        {
            var result = mainCharon.SetExtendedActivePort(addressOfCharonWithThisPort, portNumber);
            if (result == CharonOperationResult.Ok)
                return true;
            var vm = new MyMessageBoxViewModel(MessageType.Error, $@"{mainCharon.LastErrorMessage}");
            _windowManager.ShowDialogWithAssignedOwner(vm);
            return false;
        }


        public bool CanMeasurementClientAction(object param)
        {
            if (_currentUser.Role > Role.Operator)
                return false;

            if (param is TraceLeaf traceLeaf && !traceLeaf.IsInZone) return false;

            var parent = GetParent(param);
            if (parent == null)
                return false;

            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            return rtuLeaf.IsAvailable;
        }

        public bool CanMeasurementRftsReflectAction(object param)
        {
            if (_currentUser.Role > Role.Operator)
                return false;

            if (param is TraceLeaf traceLeaf && !traceLeaf.IsInZone) return false;

            var parent = GetParent(param);
            if (parent == null)
                return false;

            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            return rtuLeaf.IsAvailable && rtuLeaf.MonitoringState == MonitoringState.Off;
        }

    }
}