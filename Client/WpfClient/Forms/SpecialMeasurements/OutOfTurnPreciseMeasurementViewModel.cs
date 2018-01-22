﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class OutOfTurnPreciseMeasurementViewModel : Screen
    {
        public TraceLeaf TraceLeaf { get; set; }
        public RtuLeaf RtuLeaf { get; set; }
        private readonly IMyLog _logFile;
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;

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

        public OutOfTurnPreciseMeasurementViewModel(IMyLog logFile, ReadModel readModel, IWcfServiceForClient c2DWcfManager)
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
        }

        public void Initialize(TraceLeaf traceLeaf)
        {
            TraceLeaf = traceLeaf;
        }

        protected override async void OnViewLoaded(object view)
        {
            IsOpen = true;
            IsCancelButtonEnabled = false;
            DisplayName = Resources.SID_Precise_monitoring_out_of_turn;

            await StartRequestedMeasurement();

//            var preciseDuration = TraceLeaf.BaseRefsSet.PreciseDuration.ToString(@"mm\:ss");
//            Message = $@"Precise measurement on trace {TraceLeaf.Title} in progress. Wait please... ({preciseDuration})";
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

        private async Task<OutOfTurnMeasurementStartedDto> StartRequestedMeasurement()
        {
            Message = Resources.SID_Sending_command__Wait_please___;

            var parent = (IPortOwner)TraceLeaf.Parent;
            RtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)TraceLeaf.Parent.Parent;

            var dto = new DoOutOfTurnPreciseMeasurementDto()
            {
                RtuId = RtuLeaf.Id,
                PortWithTraceDto = new PortWithTraceDto()
                {
                    OtauPort = new OtauPortDto()
                    {
                        OtauIp = parent.OtauNetAddress.Ip4Address,
                        OtauTcpPort = parent.OtauNetAddress.Port,
                        OpticalPort = TraceLeaf.PortNumber,
                        IsPortOnMainCharon = parent is RtuLeaf,
                    },
                    TraceId = TraceLeaf.Id,
                }
            };
            return await _c2DWcfManager.DoOutOfTurnPreciseMeasurementAsync(dto);
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
            await InterruptOutOfTurnPreciseMeasurement();
            TryClose();
        }

        private async Task InterruptOutOfTurnPreciseMeasurement()
        {
            _logFile.AppendLine(@"Interrupting OutOfTurnPreciseMonitoring...");
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == RtuLeaf.Id);
            if (rtu == null) return;

            var dto = new InitializeRtuDto()
            {
                RtuId = RtuLeaf.Id,
                RtuAddresses = new DoubleAddress() { Main = rtu.MainChannel, HasReserveAddress = rtu.IsReserveChannelSet, Reserve = rtu.ReserveChannel},
                ShouldMonitoringBeStopped = false,
            };
            await _c2DWcfManager.InitializeRtuAsync(dto);
        }
    }
}
