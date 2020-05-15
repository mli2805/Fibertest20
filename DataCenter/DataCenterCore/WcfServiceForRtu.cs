using System;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForRtu : IWcfServiceForRtu
    {
        private readonly IMyLog _logFile;
        private readonly ClientsCollection _clientsCollection;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly D2CWcfManager _d2CWcfManager;
        private readonly GlobalState _globalState;

        public WcfServiceForRtu(IMyLog logFile, ClientsCollection clientsCollection,
            RtuStationsRepository rtuStationsRepository, D2CWcfManager d2CWcfManager, GlobalState globalState)
        {
            _logFile = logFile;
            _clientsCollection = clientsCollection;
            _rtuStationsRepository = rtuStationsRepository;
            _d2CWcfManager = d2CWcfManager;
            _globalState = globalState;

            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"RTU listener: works in thread {tid}");
        }

        public void NotifyUserCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            try
            {
                if (_globalState.IsDatacenterInDbOptimizationMode)
                    return;

                if (_clientsCollection.HasAnyWebClients())
                    SendMoniStepToWebApi(dto).Wait();
                var addresses = _clientsCollection.GetDesktopClientsAddresses();
                if (addresses == null)
                    return;
                _d2CWcfManager.SetClientsAddresses(addresses);
                _d2CWcfManager.NotifyUsersRtuCurrentMonitoringStep(dto).Wait();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("WcfServiceForRtu.NotifyUserCurrentMonitoringStep: " + e.Message);
            }
        }

        public void RegisterRtuHeartbeat(RtuChecksChannelDto dto)
        {
            try
            {
                _rtuStationsRepository.RegisterRtuHeartbeatAsync(dto).Wait();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("WcfServiceForRtu.RegisterRtuHeartbeat: " + e.Message);
            }
        }

        public void TransmitClientMeasurementResult(ClientMeasurementDoneDto result)
        {
            _logFile.AppendLine($"Measurement Client result received ({result.SorBytes?.Length} bytes, for clientIp {result.ClientIp})");

            if (_globalState.IsDatacenterInDbOptimizationMode)
                return;

            if (result.SorBytes == null || result.SorBytes.Length == 0) return;
            try
            {
                var clientStation = _clientsCollection.GetClientStation(result.ClientIp);
                if (clientStation == null) return;
                if (clientStation.IsWebClient)
                {
                    SendClientMeasResultToWebApi(result).Wait();
                }
                else
                {
                    var addresses = _clientsCollection.GetDesktopClientsAddresses(result.ClientIp);
                    if (addresses == null)
                        return;
                    _d2CWcfManager.SetClientsAddresses(addresses);
                    _d2CWcfManager.NotifyMeasurementClientDone(result).Wait();
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("WcfServiceForRtu.TransmitClientMeasurementResult: " + e.Message);
            }
        }

        private static readonly HttpClient client = new HttpClient();
        private async Task SendMoniStepToWebApi(CurrentMonitoringStepDto dto)
        {
            try
            {
                var json = JsonConvert.SerializeObject(dto);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json"); 
                var response = await client.PostAsync("http://localhost:11080/proxy/notify-monitoring-step", stringContent);
                await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
            }
        }

        private async Task<string> SendClientMeasResultToWebApi(ClientMeasurementDoneDto dto)
        {
            try
            {
                _logFile.AppendLine($"SendMoniStepToWebApi for {dto.ClientIp}");
                var json = JsonConvert.SerializeObject(dto);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json"); 
                var response = await client.PostAsync("http://localhost:11080/proxy/client-measurement-done", stringContent);
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }
    }
}
