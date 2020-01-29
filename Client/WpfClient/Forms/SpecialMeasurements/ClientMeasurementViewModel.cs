using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class ClientMeasurementViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly OnDemandMeasurement _onDemandMeasurement;
        private readonly IWcfServiceForC2R _c2RWcfManager;
        private readonly IWindowManager _windowManager;
        public RtuLeaf RtuLeaf { get; set; }
        private DoClientMeasurementDto _dto;

        public bool IsOpen { get; set; }

        private string _message = "";
        public string Message
        {
            get => _message;
            set
            {
                if (value == _message) return;
                _message = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isCancelButtonEnabled;
        public bool IsCancelButtonEnabled
        {
            get => _isCancelButtonEnabled;
            set
            {
                if (value == _isCancelButtonEnabled) return;
                _isCancelButtonEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public ClientMeasurementViewModel(ILifetimeScope globalScope, IMyLog logFile, OnDemandMeasurement onDemandMeasurement,
            IWcfServiceForC2R c2RWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _onDemandMeasurement = onDemandMeasurement;
            _c2RWcfManager = c2RWcfManager;
            _windowManager = windowManager;
        }

        public bool Initialize(Leaf parent, int portNumber)
        {
            RtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            var otau = (IPortOwner)parent;
            var address = otau.OtauNetAddress;

            var vm = _globalScope.Resolve<OtdrParametersThroughServerSetterViewModel>();
            vm.Initialize(RtuLeaf.TreeOfAcceptableMeasParams);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
            if (!vm.IsAnswerPositive)
                return false;

            _dto = new DoClientMeasurementDto()
            {
                RtuId = RtuLeaf.Id,
                OtauPortDto = new OtauPortDto()
                {
                    //                    OtauIp = address.Ip4Address,
                    //                    OtauTcpPort = address.Port,
                    Serial = otau.Serial,
                    IsPortOnMainCharon = RtuLeaf.OtauNetAddress.Equals(address),
                    OpticalPort = portNumber
                },
                OtauIp = address.Ip4Address,
                OtauTcpPort = address.Port,
                SelectedMeasParams = vm.GetSelectedParameters(),
            };
            return true;
        }

        protected override async void OnViewLoaded(object view)
        {
            IsOpen = true;
            IsCancelButtonEnabled = false;
            DisplayName = Resources.SID_Measurement__Client_;

            var result = await StartRequestedMeasurement();
            if (result.ReturnCode != ReturnCode.Ok)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, result.ExceptionMessage);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                TryClose();
                return;
            }

            Message = Resources.SID_Measurement__Client__in_progress__Please_wait___;
            IsCancelButtonEnabled = true;
        }

        private async Task<ClientMeasurementStartedDto> StartRequestedMeasurement()
        {
            Message = Resources.SID_Sending_command__Wait_please___;
            return await _c2RWcfManager.DoClientMeasurementAsync(_dto);
        }

        public void ShowReflectogram(byte[] sorBytes)
        {
            _logFile.AppendLine(@"Measurement (Client) result received");
            var clientPath = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory);
            var filename = clientPath + $@"\temp\meas-{DateTime.Now:yyyy-MM-dd-hh-mm-ss}.sor";
            SorData.Save(sorBytes, filename);
            var iitPath = FileOperations.GetParentFolder(clientPath);
            Process.Start(iitPath + @"\RftsReflect\Reflect.exe", filename);
            TryClose(true);
        }

        public override void CanClose(Action<bool> callback)
        {
            IsOpen = false;
            callback(true);
        }

        public async void Cancel()
        {
            Message = Resources.SID_Interrupting_Measurement__Client___Wait_please___;
            IsCancelButtonEnabled = false;
            await _onDemandMeasurement.Interrupt(RtuLeaf, @"measurement (Client)");
            TryClose();
        }


    }
}
