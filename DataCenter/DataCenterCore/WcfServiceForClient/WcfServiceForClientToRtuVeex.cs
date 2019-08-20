using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceForClient
    {
        public async Task<RtuInitializedDto> InitializeRtuVeexAsync(InitializeRtuDto dto)
        {
            return await _clientToRtuTransmitter.InitializeAsync(dto);
        }

    }
}
