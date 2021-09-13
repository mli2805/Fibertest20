using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;


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
        private readonly IWcfServiceCommonC2D _c2RWcfManager;

        public CommonActions(IniFile iniFile35, IMyLog logFile, CurrentUser currentUser,
            Model readModel, IWindowManager windowManager, IWcfServiceCommonC2D c2RWcfManager,
            ClientMeasurementViewModel clientMeasurementViewModel)
        {
            _iniFile35 = iniFile35;
            _logFile = logFile;
            _currentUser = currentUser;
            _clientMeasurementViewModel = clientMeasurementViewModel;
            _readModel = readModel;
            _windowManager = windowManager;
            _c2RWcfManager = c2RWcfManager;
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
                DoMeasurementRftsReflect(parent, GetPortNumber(param));
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
         * New RTU (MAK100) has main address 172.16.5.53 and its otau could be addressed by the same address,
         * but in otau address stored value 192.168.88.101
         *      mainCharon = 172.16.5.53 : 23    addressOfCharonWithThisPort = 172.16.5.53 : 23
         *          and than otdr address = 172.16.5.53 : 1500
         *
         * Every of these RTU could be augmented with BOP (let it be that additional otau has address 172.16.5.57 : 11842)
         * but OTDR is always part of RTU, not of BOP
         *
         *      so Old RTU with BOP:  mainCharon = 192.168.96.59 : 23      addressOfCharonWithThisPort = 172.16.5.57 : 11842
         *          and than otdr address = 192.168.96.59 : 1500
         *
         *      while MAK100 with BOP:   mainCharon = 172.16.5.53 : 23      addressOfCharonWithThisPort = 172.16.5.57 : 11842
         *          and than otdr address = 172.16.5.53 : 1500
         *
          * UCC (БУС) has main address 172.16.4.8
         *  and Fibertest Client addresses it 172.16.4.8 : 11842
         *  its OTAU could be addressed
         *  - from UCC as 192.168.88.101 : 23
         *  - from outside as 172.16.4.8 : 23
         *  but its OTDR is a separate device (АТР), which could be addressed
         *  - from UCC as 192.168.88.102 : 10001
         *  - from outside as 172.16.4.8 : 10001
         *
         *
         */
        private void DoMeasurementRftsReflect(Leaf parent, int portNumber)
        {
            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            var isMak100 = rtuLeaf.OtauNetAddress.Ip4Address == @"192.168.88.101";
            var isUcc = rtuLeaf.OtauNetAddress.Ip4Address == @"192.168.88.102"; // БУС
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuLeaf.Id);
            if (rtu == null) return;

            var mainCharonAddress = isMak100 || isUcc
                ? (NetAddress)rtu.MainChannel.Clone()
                : (NetAddress)rtu.OtdrNetAddress.Clone();
            mainCharonAddress.Port = 23;
            var mainCharon = new Charon(mainCharonAddress, true, _iniFile35, _logFile)
            {
                OwnPortCount = rtuLeaf.OwnPortCount,
                Serial = rtuLeaf.Serial,
            };

            var otauId = rtu.OtauId;
            var masterPort = 0;
            string serialOfCharonWithThisPort;
            if (parent is OtauLeaf otauLeaf)
            {
                serialOfCharonWithThisPort = otauLeaf.Serial;

                var bopCharon = new Charon(otauLeaf.OtauNetAddress, false, _iniFile35, _logFile);
                bopCharon.Serial = otauLeaf.Serial;
                bopCharon.OwnPortCount = otauLeaf.OwnPortCount;
                mainCharon.Children = new Dictionary<int, Charon> { { otauLeaf.MasterPort, bopCharon } };

                otauId = _readModel.Otaus.First(o => o.Serial == otauLeaf.Serial).VeexOtauId;
                masterPort = otauLeaf.MasterPort;
            }
            else
            {
                serialOfCharonWithThisPort = mainCharon.Serial;
            }

            var toggleResult = rtu.RtuMaker == RtuMaker.IIT
                ? ToggleToPort(mainCharon, serialOfCharonWithThisPort, portNumber)
                : VeexToggleToPort(otauId, masterPort, portNumber);

            if (!toggleResult) return;

            int otdrPort = isUcc ? 10001 : 1500;
            var rootPath = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory, 2);
            System.Diagnostics.Process.Start(rootPath + @"\RftsReflect\Reflect.exe",
                $@"-fnw -n {mainCharonAddress.Ip4Address} -p {otdrPort}");
        }

        private bool ToggleToPort(Charon mainCharon, string serialOfCharonWithThisPort, int portNumber)
        {
            var result = mainCharon.SetExtendedActivePort(serialOfCharonWithThisPort, portNumber);
            if (result == CharonOperationResult.Ok)
                return true;
            var vm = new MyMessageBoxViewModel(MessageType.Error, $@"{mainCharon.LastErrorMessage}");
            _windowManager.ShowDialogWithAssignedOwner(vm);
            return false;
        }

        private bool VeexToggleToPort(string otauId, int masterPortNumber, int portNumber)
        {
            _c2RWcfManager.PrepareReflectMeasurementAsync(new PrepareReflectMeasurementDto()
            {
                OtauId = otauId,
                MasterPortNumber = masterPortNumber,
                PortNumber = portNumber,
            });
            return true;
        }


        public bool CanMeasurementClientAction(object param)
        {
            if (_currentUser.Role > Role.Operator)
                return false;

            if (param is TraceLeaf traceLeaf && !traceLeaf.IsInZone) return false;

            var parent = GetParent(param);
            if (parent == null)
                return false;

            if (parent is OtauLeaf otauLeaf && otauLeaf.OtauState != RtuPartState.Ok)
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

            if (parent is OtauLeaf otauLeaf && otauLeaf.OtauState != RtuPartState.Ok)
                return false;

            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            return rtuLeaf.IsAvailable && rtuLeaf.MonitoringState == MonitoringState.Off;
        }

    }
}