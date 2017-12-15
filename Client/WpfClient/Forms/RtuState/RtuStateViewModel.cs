using System;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuStateViewModel : Screen
    {
        private readonly SoundManager _soundManager;
        private bool _isSoundForThisVmInstanceOn;

        public RtuStateVm Model { get; set; }

        public RtuStateViewModel(SoundManager soundManager)
        {
            _soundManager = soundManager;
        }

        public void Initialize(RtuStateVm model)
        {
            Model = model;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_State_of_RTU;
        }

        public void NotifyUserMonitoringResult(Measurement dto)
        {
            var portLineVm = Model.Ports.FirstOrDefault(p => p.TraceId == dto.TraceId);
            if (portLineVm == null)
                return;

            portLineVm.TraceState = dto.BaseRefType == BaseRefType.Fast && dto.TraceState != FiberState.Ok? FiberState.Suspicion : dto.TraceState;
            portLineVm.Timestamp = dto.MeasurementTimestamp;

            Model.SetWorstTraceStateAsAggregate();
        }

        public void NotifyUserCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            string portName = "";
            string traceTitle = "";

            if (dto.PortWithTraceDto != null)
            {
                portName = dto.PortWithTraceDto.OtauPort.IsPortOnMainCharon
                    ? $@"{dto.PortWithTraceDto.OtauPort.OpticalPort}"
                    : $@"{dto.PortWithTraceDto.OtauPort.OtauIp}:{dto.PortWithTraceDto.OtauPort.OtauTcpPort}-{
                            dto.PortWithTraceDto.OtauPort.OpticalPort
                        }";

                var portLineVm = Model.Ports.FirstOrDefault(p => p.TraceId == dto.PortWithTraceDto.TraceId);
                if (portLineVm != null)
                {
                    traceTitle = portLineVm.TraceTitle;
                    portName = portLineVm.Number;
                }
            }

            Model.CurrentMeasurementStep = BuildMessage(dto.State, portName, traceTitle);
        }

        private string BuildMessage(RtuCurrentState state, string portName, string traceTitle)
        {
            switch (state)
            {
                case RtuCurrentState.Idle:
                    return Resources.SID_Is_waiting_for_the_command;
                case RtuCurrentState.Toggle:
                   return string.Format(Resources.SID_Toggling_to_the_port__0_, portName);
                case RtuCurrentState.Measure:
                    return string.Format(Resources.SID_Measurement_on_port__0___trace___1__, portName, traceTitle);
                case RtuCurrentState.Analysis:
                    return string.Format(Resources.SID_Measurement_s_result_analysis__port__0____trace___1__, portName, traceTitle);
                case RtuCurrentState.Interrupted:
                    return Resources.SID_Measurement_interrupted;
                default:
                    return Resources.SID_Unknown;
            }
        }

        public void TurnSoundOff()
        {
            if (_isSoundForThisVmInstanceOn)
                _soundManager.StopAlert();
            _isSoundForThisVmInstanceOn = false;
            Model.IsSoundButtonEnabled = false;
        }

        public override void CanClose(Action<bool> callback)
        {
            if (_isSoundForThisVmInstanceOn)
                _soundManager.StopAlert();
            callback(true);
        }
        public void Close()
        {
            TryClose();
        }
    }
}
