using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.RtuManagement
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

        private void SendCurrentMonitoringStep(RtuCurrentMonitoringStep currentMonitoringStep, MonitorigPort monitorigPort, BaseRefType baseRefType = BaseRefType.None)
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
                    OtauIp = monitorigPort.NetAddress.Ip4Address,
                    OtauTcpPort = monitorigPort.NetAddress.Port,
                    OpticalPort = monitorigPort.OpticalPort
                },
                BaseRefType = baseRefType,
            };

            var thread = new Thread(SendCurrentMonitoringStepThread) { IsBackground = true };
            thread.Start(dto);
        }

        private void SendCurrentMonitoringStepThread(object dto)
        {
            var step = dto as KnowRtuCurrentMonitoringStepDto;
//            new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendCurrentMonitoringStep(step);
            IsSenderBusy = false;
        }

    }
}

