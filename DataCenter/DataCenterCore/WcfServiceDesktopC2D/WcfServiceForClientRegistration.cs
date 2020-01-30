using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceDesktopC2D
    {
       
        public async Task<int> UnregisterClientAsync(UnRegisterClientDto dto)
        {
            _clientsCollection.UnregisterClientAsync(dto);
            _logFile.AppendLine($"Client {dto.Username} from {dto.ClientIp} exited");

            var command = new UnregisterClientStation();
            await _eventStoreService.SendCommand(command, dto.Username, dto.ClientIp);

            return 0;
        }

        public async Task<bool> CheckServerConnection(CheckServerConnectionDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} checked server connection");
            await Task.Delay(1);
            return true;
        }
    }
}
