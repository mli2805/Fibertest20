using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class OutOfTurnPreciseMeasurementViewModel : Screen
    {
        public TraceLeaf TraceLeaf { get; set; }
        private IPortOwner _portOwner;
        private Rtu _rtu;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly MeasurementInterrupter _measurementInterrupter;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;
        private readonly IWindowManager _windowManager;

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

        public OutOfTurnPreciseMeasurementViewModel(CurrentUser currentUser, Model readModel, MeasurementInterrupter measurementInterrupter, 
            IWcfServiceCommonC2D c2RWcfManager, IWindowManager windowManager)
        {
            _currentUser = currentUser;
            _readModel = readModel;
            _measurementInterrupter = measurementInterrupter;
            _c2RWcfManager = c2RWcfManager;
            _windowManager = windowManager;
        }

        public void Initialize(TraceLeaf traceLeaf)
        {
            TraceLeaf = traceLeaf;
            _portOwner = (IPortOwner)TraceLeaf.Parent;
            var rtuLeaf = _portOwner is RtuLeaf leaf ? leaf : (RtuLeaf)TraceLeaf.Parent.Parent;
            _rtu = _readModel.Rtus.First(r => r.Id == rtuLeaf.Id);
        }

        protected override async void OnViewLoaded(object view)
        {
            IsOpen = true;
            IsCancelButtonEnabled = false;
            DisplayName = Resources.SID_Precise_monitoring_out_of_turn;

            var result = await StartRequestedMeasurement();
            if (result.ReturnCode != ReturnCode.Ok && result.ReturnCode != ReturnCode.InProgress)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, result.ErrorMessage);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                TryClose();
                return;
            }

            Message = BuildMeasurementMessage();
            IsCancelButtonEnabled = true;
        }

        private string BuildMeasurementMessage()
        {
            var line1 = Resources.SID_Precise_monitoring_in_progress_ + Environment.NewLine;
            var line2 = string.Format(Resources.SID_Trace____0___, TraceLeaf.Title) + Environment.NewLine;
            var preciseDuration = TraceLeaf.BaseRefsSet.PreciseDuration.ToString(@"mm\:ss");
            var line3 = string.Format(Resources.SID_Measurement_time_accordingly_to_base_ref_is__0_, preciseDuration);
            return line1 + line2 + line3;
        }

        private async Task<RequestAnswer> StartRequestedMeasurement()
        {
            Message = Resources.SID_Sending_command__Wait_please___;
          
            var dto = new DoOutOfTurnPreciseMeasurementDto()
            {
                ConnectionId = _currentUser.ConnectionId,
                RtuId = _rtu.Id,
                PortWithTraceDto = new PortWithTraceDto()
                {
                    OtauPort = new OtauPortDto()
                    {
                        Serial = _portOwner.Serial,
                        OpticalPort = TraceLeaf.PortNumber,
                        IsPortOnMainCharon = _portOwner is RtuLeaf,
                    },
                    TraceId = TraceLeaf.Id,
                }
            };
            return await _c2RWcfManager.DoOutOfTurnPreciseMeasurementAsync(dto);
        }

        public override void CanClose(Action<bool> callback)
        {
            IsOpen = false;
            callback(true);
        }

        public async void Cancel()
        {
            Message = Resources.SID_Interrupting_out_of_turn_monitoring__Wait_please___;
            IsCancelButtonEnabled = false;
            await _measurementInterrupter.Interrupt(_rtu, @"out of turn precise monitoring");
            TryClose();
        }
    }
}
