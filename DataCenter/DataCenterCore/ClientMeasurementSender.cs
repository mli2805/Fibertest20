using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public  class ClientMeasurementSender
    {
        private readonly IMyLog _logFile;
        private readonly ClientsCollection _clientsCollection;
        private readonly D2CWcfManager _d2CWcfManager;
        private readonly MeasurementsForWebNotifier _measurementsForWebNotifier;

        public ClientMeasurementSender(IMyLog logFile, ClientsCollection clientsCollection,
            D2CWcfManager d2CWcfManager, MeasurementsForWebNotifier measurementsForWebNotifier)
        {
            _logFile = logFile;
            _clientsCollection = clientsCollection;
            _d2CWcfManager = d2CWcfManager;
            _measurementsForWebNotifier = measurementsForWebNotifier;
        }

        public  async Task ToClient(ClientMeasurementResultDto result)
        {
            var word = (result.SorBytes == null || result.SorBytes.Length == 0)
                ? $": {result.ReturnCode}"
                : $", {result.SorBytes.Length} bytes";
            _logFile.AppendLine($"Measurement Client result for {_clientsCollection.Get(result.ConnectionId)}{word}");

            try
            {
                var client = _clientsCollection.Get(result.ConnectionId);
                if (client == null)
                {
                    _logFile.AppendLine($@"TransmitClientMeasurementResult: client {result.ConnectionId} not found");
                    return;
                }

                if (client.IsWebClient)
                {
                    _measurementsForWebNotifier.Push(result);
                    _logFile.AppendLine("TransmitClientMeasurementResult: measurement placed into queue for web clients");
                }
                else
                {
                    _d2CWcfManager.SetClientsAddresses(new List<DoubleAddress>()
                    {
                        new DoubleAddress()
                            { Main = new NetAddress(client.ClientIp, client.ClientAddressPort) }
                    });
                    await _d2CWcfManager.NotifyMeasurementClientDone(result);
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("WcfServiceForRtu.TransmitClientMeasurementResult: " + e.Message);
            }
            _logFile.AppendLine("Client measurement ended");
        }
    }
}