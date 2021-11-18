using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public class ClientsCollection
    {
        public readonly IniFile IniFile;
        public readonly IMyLog LogFile;
        public readonly Model WriteModel;
        public readonly CurrentDatacenterParameters CurrentDatacenterParameters;
        private readonly EventStoreService _eventStoreService;
        public readonly D2CWcfManager D2CWcfService;
        public readonly IFtSignalRClient FtSignalRClient;
        public readonly List<ClientStation> Clients = new List<ClientStation>();

        public ClientsCollection(IniFile iniFile, IMyLog logFile, Model writeModel,
            CurrentDatacenterParameters currentDatacenterParameters, EventStoreService eventStoreService,
            D2CWcfManager d2CWcfService, IFtSignalRClient ftSignalRClient)
        {
            IniFile = iniFile;
            LogFile = logFile;
            WriteModel = writeModel;
            CurrentDatacenterParameters = currentDatacenterParameters;
            _eventStoreService = eventStoreService;
            D2CWcfService = d2CWcfService;
            FtSignalRClient = ftSignalRClient;
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            // R1
            var licenseCheckResult = this.CheckLicense(dto);
            if (licenseCheckResult != null)
            {
                LogFile.AppendLine(licenseCheckResult.ReturnCode.GetLocalizedString());
                return licenseCheckResult;
            }

            // R2
            var user = WriteModel.Users
                .FirstOrDefault(u => u.Title == dto.UserName 
                                     && u.EncodedPassword == dto.Password);
            if (user == null)
                return new ClientRegisteredDto { ReturnCode = ReturnCode.NoSuchUserOrWrongPassword };

            // R3
            var hasRight = user.CheckRights(dto);
            if (hasRight != null)
                return hasRight;

           
            // R4
            var theSameUserCheckResult = await this.CheckTheSameUser(dto, user);
            if (theSameUserCheckResult != null)
                return theSameUserCheckResult;
           

            // R5 Machine Key
            var machineKeyCheckResult = this.CheckMachineKey(dto, user);
            if (machineKeyCheckResult != null)
                return machineKeyCheckResult;
          
            Clients.Add(ClientStationFactory.Create(dto, user));
            LogFile.AppendLine($"Client {dto.UserName}/{dto.ClientIp} registered with connectionId {dto.ConnectionId}");
            if (!dto.IsWebClient)
                LogStations();
            return this.FillInSuccessfulResult(dto, user);
        }

        public async Task<bool> ChangeGuidWithSignalrConnectionId(string oldGuid, string connId)
        {
            await Task.Delay(1);
            var station = Clients.FirstOrDefault(c => c.ConnectionId == oldGuid);
            if (station == null)
                return false;
            station.ConnectionId = connId;
            station.LastConnectionTimestamp = DateTime.Now;
            LogStations();
            return true;
        }

        public bool RegisterHeartbeat(string connectionId)
        {
            var station = Clients.FirstOrDefault(c => c.ConnectionId == connectionId);
            if (station != null)
                station.LastConnectionTimestamp = DateTime.Now;
            return station != null;
        }

        // if user just closed the browser tab instead of logging out
        // WebApi has not got user's name and put it as "onSignalRDisconnected"
        public bool UnregisterClientAsync(UnRegisterClientDto dto)
        {
            if (dto.ConnectionId == FtSignalRClient.ServerConnectionId) // it is DC to WebApi connection
                return false;
            var station = Clients.FirstOrDefault(s => s.ConnectionId == dto.ConnectionId);
            if (station != null)
            {
                Clients.Remove(station);
                LogFile.AppendLine($"Client {dto.Username}/{dto.ClientIp} with connectionId {dto.ConnectionId} unregistered.");
                LogStations();
                return true;
            }

            LogFile.AppendLine($"There is no client {dto.Username}/{dto.ClientIp} with connectionId {dto.ConnectionId}");
            LogStations();
            return false;
        }

        public async void CleanDeadClients(TimeSpan timeSpan)
        {
            DateTime noLaterThan = DateTime.Now - timeSpan;
            var deadStations = Clients.Where(s => s.LastConnectionTimestamp < noLaterThan).ToList();
            if (deadStations.Count == 0) return;

            foreach (var deadStation in deadStations)
            {
                LogFile.AppendLine($"Dead client {deadStation.UserName}/{deadStation.ClientIp} with connectionId {deadStation.ConnectionId} and last checkout time {deadStation.LastConnectionTimestamp:T} removed.");

                var command = new LostClientConnection();
                await _eventStoreService.SendCommand(command, deadStation.UserName, deadStation.ClientIp);

                Clients.Remove(deadStation);
            }
            LogStations();
        }

        private void LogStations()
        {
            LogFile.EmptyLine();
            LogFile.AppendLine($"There are {Clients.Count} client(s):");
            LogFile.EmptyLine('-');
            foreach (var station in Clients)
            {
                LogFile.AppendLine($"{station.UserName}/{station.ClientIp}:{station.ClientAddressPort} with connection id {station.ConnectionId}");
            }
            LogFile.EmptyLine('-');
            LogFile.EmptyLine();
        }
    }
}
