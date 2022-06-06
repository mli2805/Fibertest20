using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IniFile _iniFile;
        private readonly Model _readModel;
        private readonly MeasurementInterruptor _measurementInterruptor;
        private readonly MeasurementDtoProvider _measurementDtoProvider;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly VeexMeasurementFetcher _veexMeasurementFetcher;
        private readonly ReflectogramManager _reflectogramManager;
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

        public ClientMeasurementViewModel(ILifetimeScope globalScope, IniFile iniFile, Model readModel,
            MeasurementInterruptor measurementInterruptor, MeasurementDtoProvider measurementDtoProvider,
            IWcfServiceCommonC2D c2RWcfManager, IWindowManager windowManager, 
            VeexMeasurementFetcher veexMeasurementFetcher,
            ReflectogramManager reflectogramManager)
        {
            _globalScope = globalScope;
            _iniFile = iniFile;
            _readModel = readModel;
            _measurementInterruptor = measurementInterruptor;
            _measurementDtoProvider = measurementDtoProvider;
            _c2RWcfManager = c2RWcfManager;
            _windowManager = windowManager;
            _veexMeasurementFetcher = veexMeasurementFetcher;
            _reflectogramManager = reflectogramManager;
        }

        public bool Initialize(Leaf parent, int portNumber)
        {
            RtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            _rtu = _readModel.Rtus.First(r => r.Id == RtuLeaf.Id);

            _vm = _globalScope.Resolve<OtdrParametersThroughServerSetterViewModel>();
            _vm.Initialize(_rtu.AcceptableMeasParams, _iniFile);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(_vm);
            if (!_vm.IsAnswerPositive)
                return false;

            _dto = _measurementDtoProvider
                .Initialize(parent, portNumber, false)
                .PrepareDto(false, _vm.GetSelectedParameters(), _vm.GetVeexSelectedParameters());

            return true;
        }

        public DoClientMeasurementDto ForUnitTests(Leaf parent, int portNumber,
            List<MeasParamByPosition> iitMeasParams, VeexMeasOtdrParameters veexMeasParams)
        {
            return _measurementDtoProvider
                .Initialize(parent, portNumber, false)
                .PrepareDto(false, iitMeasParams, veexMeasParams);
        }

        protected override async void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Measurement__Client_;
            IsOpen = true;
            IsCancelButtonEnabled = false;

            Message = Resources.SID_Sending_command__Wait_please___;
            var startResult = await _c2RWcfManager.DoClientMeasurementAsync(_dto);
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
                var veexResult = await _veexMeasurementFetcher.Fetch(_dto.RtuId, startResult.ClientMeasurementId);
                if (veexResult.CompletedStatus == MeasurementCompletedStatus.MeasurementCompletedSuccessfully)
                    ShowReflectogram(veexResult.SorBytes);
                TryClose(true);
            }
        }

        public void ShowReflectogram(byte[] sorBytes)
        {
            _reflectogramManager.ShowClientMeasurement(sorBytes);
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
            await _measurementInterruptor.Interrupt(_rtu, @"measurement (Client)");
            TryClose();
        }
    }
}
