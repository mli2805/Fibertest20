using System.Collections.Generic;
using System.Threading;
using Dto;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.UtilsLib;
using WcfConnections;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private DoubleAddressWithConnectionStats _serverAddresses;

        private readonly object _isSenderBusyLocker = new object();
        private bool _isSenderBusy;
        public bool IsSenderBusy
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

        private void SendToUserInitializationResult(CharonOperationResult result, DoubleAddress rtuDoubleAddress)
        {
            IsSenderBusy = true;

            var dto = result == CharonOperationResult.Ok
                ? new RtuInitializedDto()
                {
                    RtuId = _id,
                    IsInitialized = true,
                    PcDoubleAddress = rtuDoubleAddress,
                    OtdrAddress = _mainCharon.NetAddress,
                    Serial = _mainCharon.Serial,
                    FullPortCount = _mainCharon.FullPortCount,
                    OwnPortCount = _mainCharon.OwnPortCount,
                    Version = _version,
                    Children = new Dictionary<int, OtauDto>(),
                }
                : new RtuInitializedDto() { RtuId = _id, IsInitialized = false };
            new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendInitializationConfirm(dto);

            IsSenderBusy = false;
        }

        private void SendCurrentMonitoringStep(RtuCurrentMonitoringStep currentMonitoringStep, ExtendedPort extendedPort, BaseRefType baseRefType = BaseRefType.None)
        {
            if (IsSenderBusy)
                return;

            IsSenderBusy = true;

            var dto = new KnowRtuCurrentMonitoringStepDto()
            {
                RtuId = _id,
                MonitoringStep = currentMonitoringStep,
                OtauPort = new OtauPortDto()
                {
                    OtauIp = extendedPort.NetAddress.Ip4Address,
                    OtauTcpPort = extendedPort.NetAddress.Port,
                    OpticalPort = extendedPort.OpticalPort
                },
                BaseRefType = baseRefType,
            };

            var thread = new Thread(SendCurrentMonitoringStepThread) {IsBackground = true};
            thread.Start(dto);
        }

        private void SendCurrentMonitoringStepThread(object dto)
        {
            var step = dto as KnowRtuCurrentMonitoringStepDto;
            new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendCurrentMonitoringStep(step);
            IsSenderBusy = false;
        }

    }
}

