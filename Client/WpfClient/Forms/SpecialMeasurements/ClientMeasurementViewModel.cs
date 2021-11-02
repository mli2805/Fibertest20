using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
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
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly OnDemandMeasurement _onDemandMeasurement;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;
        private readonly IWindowManager _windowManager;
        public RtuLeaf RtuLeaf { get; set; }
        private Rtu _rtu;
        private DoClientMeasurementDto _dto;
        private OtdrParametersThroughServerSetterViewModel _vm;

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

        public ClientMeasurementViewModel(ILifetimeScope globalScope, IMyLog logFile, Model readModel,
            CurrentUser currentUser, OnDemandMeasurement onDemandMeasurement,
            IWcfServiceCommonC2D c2RWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _readModel = readModel;
            _currentUser = currentUser;
            _onDemandMeasurement = onDemandMeasurement;
            _c2RWcfManager = c2RWcfManager;
            _windowManager = windowManager;
        }

        public bool Initialize(Leaf parent, int portNumber)
        {
            RtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            var otauLeaf = (IPortOwner)parent;
            _rtu = _readModel.Rtus.First(r => r.Id == RtuLeaf.Id);

            _vm = _globalScope.Resolve<OtdrParametersThroughServerSetterViewModel>();
            _vm.Initialize(_rtu.AcceptableMeasParams);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(_vm);
            if (!_vm.IsAnswerPositive)
                return false;

            var otau = _readModel.Otaus.FirstOrDefault(o => o.Serial == otauLeaf.Serial);
            var otauPortDto = PrepareOtauPortDto(_rtu, otau, otauLeaf, portNumber);
            _dto = PrepareDto(_rtu, otauPortDto, otauLeaf.OtauNetAddress, _vm.GetSelectedParameters(), _vm.GetVeexSelectedParameters());
            return true;
        }

        public OtauPortDto PrepareOtauPortDto(Rtu rtu, Otau otau, IPortOwner otauLeaf, int portNumber)
        {
            var otauId = otau == null
                ? rtu.OtauId
                : otau.Id.ToString();

            var otauPortDto = new OtauPortDto(){
                OtauId = otauId,
                OpticalPort = portNumber,
                Serial = otauLeaf.Serial,
                IsPortOnMainCharon = otauLeaf is RtuLeaf,
                MainCharonPort = otau?.MasterPort ?? 1
            };
            return otauPortDto;
        }

        public DoClientMeasurementDto PrepareDto(Rtu rtu, OtauPortDto otauPortDto, 
            NetAddress address, List<MeasParam> iitMeasParams, VeexMeasOtdrParameters veexMeasParams)
        {
            var dto = new DoClientMeasurementDto()
            {
                ConnectionId = _currentUser.ConnectionId,
                RtuId = rtu.Id,
                OtdrId = rtu.OtdrId,
                OtauPortDto = otauPortDto,
                OtauIp = address.Ip4Address,
                OtauTcpPort = address.Port,

                SelectedMeasParams = iitMeasParams,
                VeexMeasOtdrParameters = veexMeasParams,
            };

            if (!otauPortDto.IsPortOnMainCharon && rtu.RtuMaker == RtuMaker.VeEX)
            {
                dto.MainOtauPortDto = new OtauPortDto()
                {
                    IsPortOnMainCharon = true,
                    OtauId = rtu.OtauId,
                    OpticalPort = otauPortDto.MainCharonPort,
                };
            }

            return dto;
        }

        protected override async void OnViewLoaded(object view)
        {
            IsOpen = true;
            IsCancelButtonEnabled = false;
            DisplayName = Resources.SID_Measurement__Client_;

            var startResult = await StartRequestedMeasurement();
            if (startResult.ReturnCode != ReturnCode.Ok)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, startResult.ErrorMessage);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                TryClose();
                return;
            }

            Message = Resources.SID_Measurement__Client__in_progress__Please_wait___;
            IsCancelButtonEnabled = true;

            if (_rtu.RtuMaker == RtuMaker.VeEX)
            {
                var getDto = new GetClientMeasurementDto()
                {
                    RtuId = _dto.RtuId,
                    VeexMeasurementId = startResult.ErrorMessage, // sorry, if ReturnCode is OK, ErrorMessage contains Id
                };
                while (true)
                {
                    await Task.Delay(5000);
                    var measResult = await _c2RWcfManager.GetClientMeasurementAsync(getDto);

                    if (measResult.ReturnCode != ReturnCode.Ok || measResult.VeexMeasurementStatus == @"failed")
                    {
                        var firstLine = measResult.ReturnCode != ReturnCode.Ok
                            ? measResult.ReturnCode.GetLocalizedString()
                            : @"Failed to do Measurement(Client)!";

                        var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                        {
                            firstLine,
                            "",
                            measResult.ErrorMessage,
                        }, 0);
                        _windowManager.ShowDialogWithAssignedOwner(vm);
                        TryClose(true);
                        return;
                    }
                    if (measResult.ReturnCode == ReturnCode.Ok && measResult.VeexMeasurementStatus == @"finished")
                    {
                        ShowReflectogram(measResult.SorBytes);
                        TryClose(true);
                        return;
                    }
                }
            }
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
            if (!Directory.Exists(clientPath + @"\temp"))
                Directory.CreateDirectory(clientPath + @"\temp");
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
