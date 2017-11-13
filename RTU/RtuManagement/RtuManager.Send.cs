using Iit.Fibertest.Dto;
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

        private void SendCurrentMonitoringStep(RtuCurrentMonitoringStep currentMonitoringStep,
            MonitorigPort monitorigPort, BaseRefType baseRefType = BaseRefType.None)
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

            new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendCurrentMonitoringStep(dto);

            IsSenderBusy = false;
        }

      
    }
}