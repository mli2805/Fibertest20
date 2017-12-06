using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuStateViewModel : Screen
    {
        public RtuStateVm Model { get; set; }

        public void Initialize(RtuStateVm model)
        {
            Model = model;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_State_of_RTU;
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

            string message;
            switch (dto.State)
            {
                case RtuCurrentState.Idle:
                    message = Resources.SID_Is_waiting_for_the_command;
                    break;
                case RtuCurrentState.Toggle:
                    message = string.Format(Resources.SID_Toggling_to_the_port__0_, portName);
                    break;
                case RtuCurrentState.Measure:
                    message = string.Format(Resources.SID_Measurement_on_port__0___trace___1__, portName, traceTitle);
                    break;
                case RtuCurrentState.Analysis:
                    message = string.Format(Resources.SID_Measurement_s_result_analysis__port__0____trace___1__, portName, traceTitle);
                    break;
                case RtuCurrentState.Interrupted:
                    message = Resources.SID_Measurement_interrupted;
                    break;
                default:
                    message = Resources.SID_Unknown;
                    break;
            }

            Model.CurrentMeasurementStep = message;
        }

        public void Close()
        {
            TryClose();
        }
    }
}
