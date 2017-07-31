using Dto;
using Iit.Fibertest.Utils35;

namespace WcfConnections
{
    public class D2CWcfManager
    {
        private readonly Logger35 _logger35;
        private readonly WcfFactory _wcfFactory;

        public D2CWcfManager(string clientAddress, IniFile iniFile, Logger35 logger35)
        {
            _logger35 = logger35;
            _wcfFactory = new WcfFactory(clientAddress, iniFile, logger35);
        }

        public void ConfirmRtuConnectionChecked(RtuConnectionCheckedDto dto)
        {
            var wcfConnection = _wcfFactory.CreateClientConnection();
            if (wcfConnection == null)
                return;

            wcfConnection.ConfirmRtuConnectionChecked(dto);
            _logger35.AppendLine($"Sent response on check connection with RTU {dto.RtuId}");
        }
    }
}