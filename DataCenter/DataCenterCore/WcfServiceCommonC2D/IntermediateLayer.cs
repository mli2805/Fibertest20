using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class IntermediateLayer
    {
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;

        public IntermediateLayer(
            IFtSignalRClient ftSignalRClient,
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter
            )
        {
            _ftSignalRClient = ftSignalRClient;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            var result = dto.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.InitializeAsync(dto)
                : await Task.Factory.StartNew(() => _clientToRtuVeexTransmitter.InitializeAsync(dto).Result);

            await _ftSignalRClient.NotifyAll("RtuInitialized", result.ToCamelCaseJson());
            return result;
        }
    }
}
