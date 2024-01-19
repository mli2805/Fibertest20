using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuMngr
{
    public partial class RtuManager
    {
        private CurrentMonitoringStepDto CreateStepDto(MonitoringCurrentStep currentStep,
            MonitoringPort? monitoringPort = null, BaseRefType baseRefType = BaseRefType.None)
        {
            return new CurrentMonitoringStepDto()
            {
                RtuId = _config.Value.General.RtuId,
                Step = currentStep,
                PortWithTraceDto = monitoringPort == null
                    ? null
                    : new PortWithTraceDto
                    {
                        OtauPort =
                        new OtauPortDto()
                        {
                            OpticalPort = monitoringPort.OpticalPort,
                            Serial = monitoringPort.CharonSerial,
                            IsPortOnMainCharon = monitoringPort.IsPortOnMainCharon,
                        },
                        TraceId = monitoringPort.TraceId
                    },
                BaseRefType = baseRefType,
            };
        }
    }
}
