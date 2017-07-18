using Dto;
using Iit.Fibertest.Utils35;

namespace WcfTestBench
{
    public class WcfManager
    {
        public NetAddress DataCenterNetAddress { get; set; }

        public WcfManager(NetAddress dataCenterNetAddress)
        {
            DataCenterNetAddress = dataCenterNetAddress;
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings)
        {
            var wcfConnection = ClientToServerWcfFactory.Create(DataCenterNetAddress.Ip4Address);
            if (wcfConnection == null)
                return false;
            return wcfConnection.ApplyMonitoringSettings(settings);
        }
    }
}
