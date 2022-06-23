﻿using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        private DoubleAddress _serverAddresses;

        private readonly object _isSenderBusyLocker = new object();
        private bool _isSenderBusy;

        private bool IsSenderBusy
        {
            get
            {
                lock (_isSenderBusyLocker)
                {
                    return _isSenderBusy;
                }
            }
            set
            {
                lock (_isSenderBusyLocker)
                {
                    _isSenderBusy = value;
                }
            }
        }

        private void SendCurrentMonitoringStep(MonitoringCurrentStep currentStep,
            MonitoringPort monitoringPort = null, BaseRefType baseRefType = BaseRefType.None)
        {
            if (IsSenderBusy)
                return;

            IsSenderBusy = true;

            var dto = new CurrentMonitoringStepDto()
            {
                RtuId = _id,
                Step = currentStep,
                PortWithTraceDto = monitoringPort == null ? null : new PortWithTraceDto()
                {
                    OtauPort = new OtauPortDto()
                    {
                        Serial = monitoringPort.CharonSerial,
//                        OtauIp = monitoringPort.NetAddress.Ip4Address,
//                        OtauTcpPort = monitoringPort.NetAddress.Port,
                        OpticalPort = monitoringPort.OpticalPort,
                        IsPortOnMainCharon = monitoringPort.IsPortOnMainCharon,
                    },
                    TraceId = monitoringPort.TraceId,
                },
                BaseRefType = baseRefType,
            };

            new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendCurrentMonitoringStep(dto);

            IsSenderBusy = false;
        }
    }
}