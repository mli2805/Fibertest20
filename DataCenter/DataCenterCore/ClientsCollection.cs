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
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly EventStoreService _eventStoreService;
        private readonly D2CWcfManager _d2CWcfService;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly List<ClientStation> _clients = new List<ClientStation>();

        public ClientsCollection(IniFile iniFile, IMyLog logFile, Model writeModel,
            CurrentDatacenterParameters currentDatacenterParameters, EventStoreService eventStoreService,
            D2CWcfManager d2CWcfService, IFtSignalRClient ftSignalRClient)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
            _currentDatacenterParameters = currentDatacenterParameters;
            _eventStoreService = eventStoreService;
            _d2CWcfService = d2CWcfService;
            _ftSignalRClient = ftSignalRClient;
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            // R1
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == dto.UserName && UserExt.FlipFlop(u.EncodedPassword) == dto.Password);
            if (user == null)
                return new ClientRegisteredDto { ReturnCode = ReturnCode.NoSuchUserOrWrongPassword };

            // R2
            //            var station = _clients.FirstOrDefault(s => s.ClientIp == dto.ClientIp);
            //            if (station != null && !(station.IsWebClient ^ dto.IsWebClient))
            //            {
            //                _clients.Remove(station);
            //            }

            // R3
            var hasRight = CheckUsersRights(dto, user);
            if (hasRight != null)
                return hasRight;
            // R4
            var licenseCheckResult = CheckLicense(dto);
            if (licenseCheckResult != null)
            {
                _logFile.AppendLine(licenseCheckResult.ReturnCode.GetLocalizedString());
                return licenseCheckResult;
            }

            // R5
            var stationWithTheSameUser = _clients.FirstOrDefault(s => s.UserId == user.UserId);
            if (stationWithTheSameUser != null)
            {
                // both clients are desktop
                if (!dto.IsWebClient && !stationWithTheSameUser.IsWebClient)
                {
                    _logFile.AppendLine($"The same user {dto.UserName} registered from device {stationWithTheSameUser.ClientIp}");
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.ThisUserRegisteredFromAnotherDevice };
                }
                else
                // different types of clients or both clients are web
                {
                    _logFile.AppendLine($"The same client {stationWithTheSameUser.UserName}/{stationWithTheSameUser.ClientIp} with connectionId {stationWithTheSameUser.ConnectionId} removed.");
                    // notify old station
                    var serverAsksClientToExitDto = new ServerAsksClientToExitDto()
                    {
                        ToAll = false,
                        ConnectionId = stationWithTheSameUser.ConnectionId,
                        Reason = UnRegisterReason.UserRegistersAnotherSession,

                        IsNewUserWeb = dto.IsWebClient,
                        NewAddress = dto.ClientIp,
                    };
                    await _d2CWcfService.ServerAsksClientToExit(serverAsksClientToExitDto);
                    await _ftSignalRClient.NotifyAll("ServerAsksClientToExit", serverAsksClientToExitDto.ToCamelCaseJson());
                    await Task.Delay(1000);
                    _clients.Remove(stationWithTheSameUser);
                    _logFile.AppendLine("Old client deleted");
                }
            }

            // R6 Machine Key
            if (_writeModel.IsMachineKeyRequired() && user.MachineKey != dto.MachineKey)
            {
                if (string.IsNullOrEmpty(dto.SecurityAdminPassword))
                {   // prohibited, call Security Admin to confirm 
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.WrongMachineKey };
                }

                var admin = _writeModel.Users.First(u => u.Role == Role.SecurityAdmin);
                if (admin.EncodedPassword != dto.SecurityAdminPassword)
                {   
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.WrongSecurityAdminPassword };
                }

                user.MachineKey = dto.MachineKey;
            }
            //

            _clients.Add(Create(dto, user));
            _logFile.AppendLine($"Client {dto.UserName}/{dto.ClientIp} registered with connectionId {dto.ConnectionId}");
            if (!dto.IsWebClient)
                LogStations();
            return await FillInSuccessfulResult(dto, user);
        }

        private ClientRegisteredDto CheckUsersRights(RegisterClientDto dto, User user)
        {
            if (dto.IsUnderSuperClient)
            {
                if (!user.Role.IsSuperclientPermitted())
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.UserHasNoRightsToStartSuperClient };
            }
            else if (dto.IsWebClient)
            {
                if (!user.Role.IsWebPermitted())
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.UserHasNoRightsToStartWebClient };
            }
            else
            {
                if (!user.Role.IsDesktopPermitted())
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.UserHasNoRightsToStartClient };
            }
            return null;
        }

        private ClientRegisteredDto CheckLicense(RegisterClientDto dto)
        {
            if (dto.IsUnderSuperClient)
            {
                if (_clients.Count(c => c.UserRole == Role.Superclient) >= _writeModel.GetSuperClientStationLicenseCount()
                    && _clients.All(s => s.ClientIp != dto.ClientIp))
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.SuperClientsCountExceeded };
                // if (_writeModel.License.SuperClientStationCount.ValidUntil < DateTime.Today)
                // return new ClientRegisteredDto() { ReturnCode = ReturnCode.SuperClientsCountLicenseExpired };
            }
            else if (dto.IsWebClient)
            {
                if (_clients.Count(c => c.IsWebClient) >= _writeModel.GetWebClientLicenseCount()
                    && _clients.All(s => s.ClientIp != dto.ClientIp))
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.WebClientsCountExceeded };
                // if (_writeModel.License.WebClientCount.ValidUntil < DateTime.Today)
                // return new ClientRegisteredDto() { ReturnCode = ReturnCode.WebClientsCountLicenseExpired };
            }
            else
            {
                if (_clients.Count(c => c.IsDesktopClient) >= _writeModel.GetClientStationLicenseCount()
                    && _clients.All(s => s.ClientIp != dto.ClientIp))
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.ClientsCountExceeded };
                // if (_writeModel.License.ClientStationCount.ValidUntil < DateTime.Today)
                // return new ClientRegisteredDto() { ReturnCode = ReturnCode.ClientsCountLicenseExpired };
            }
            return null;
        }

        private static ClientStation Create(RegisterClientDto dto, User user)
        {
            return new ClientStation()
            {
                UserId = user.UserId,
                UserName = dto.UserName,
                UserRole = user.Role,
                ClientIp = dto.Addresses.Main.GetAddress(),
                ClientAddressPort = dto.Addresses.Main.Port,
                ConnectionId = dto.ConnectionId,

                IsUnderSuperClient = dto.IsUnderSuperClient,
                IsWebClient = dto.IsWebClient,
                IsDesktopClient = !dto.IsUnderSuperClient && !dto.IsWebClient,

                LastConnectionTimestamp = DateTime.Now,
            };
        }

#pragma warning disable 1998
        private async Task<ClientRegisteredDto> FillInSuccessfulResult(RegisterClientDto dto, User user)
#pragma warning restore 1998
        {
            var result = new ClientRegisteredDto();
            result.UserId = user.UserId;
            result.Role = user.Role;
            var zone = _writeModel.Zones.First(z => z.ZoneId == user.ZoneId);
            result.ZoneId = zone.ZoneId;
            result.ZoneTitle = zone.Title;
            result.ConnectionId = dto.ConnectionId;
            result.DatacenterVersion = _currentDatacenterParameters.DatacenterVersion;
            result.ReturnCode = ReturnCode.ClientRegisteredSuccessfully;
            result.IsWithoutMapMode = _iniFile.Read(IniSection.Server, IniKey.IsWithoutMapMode, false);
            result.Smtp = _currentDatacenterParameters.Smtp;
            result.GsmModemComPort = _currentDatacenterParameters.GsmModemComPort;
            result.Snmp = _currentDatacenterParameters.Snmp;
            return result;
        }

        public async Task<bool> ChangeGuidWithSignalrConnectionId(string oldGuid, string connId)
        {
            await Task.Delay(1);
            var station = _clients.FirstOrDefault(c => c.ConnectionId == oldGuid);
            if (station == null)
                return false;
            station.ConnectionId = connId;
            station.LastConnectionTimestamp = DateTime.Now;
            LogStations();
            return true;
        }

        public bool RegisterHeartbeat(string connectionId)
        {
            var station = _clients.FirstOrDefault(c => c.ConnectionId == connectionId);
            if (station != null)
                station.LastConnectionTimestamp = DateTime.Now;
            return station != null;
        }

        // if user just closed the browser tab instead of logging out
        // WebApi has not got user's name and put it as "onSignalRDisconnected"
        public bool UnregisterClientAsync(UnRegisterClientDto dto)
        {
            if (dto.ConnectionId == _ftSignalRClient.ServerConnectionId) // it is DC to WebApi connection
                return false;
            var station = _clients.FirstOrDefault(s => s.ConnectionId == dto.ConnectionId);
            if (station != null)
            {
                _clients.Remove(station);
                _logFile.AppendLine($"Client {dto.Username}/{dto.ClientIp} with connectionId {dto.ConnectionId} unregistered.");
                LogStations();
                return true;
            }

            _logFile.AppendLine($"There is no client {dto.Username}/{dto.ClientIp} with connectionId {dto.ConnectionId}");
            LogStations();
            return false;
        }

        public async void CleanDeadClients(TimeSpan timeSpan)
        {
            DateTime noLaterThan = DateTime.Now - timeSpan;
            var deadStations = _clients.Where(s => s.LastConnectionTimestamp < noLaterThan).ToList();
            if (deadStations.Count == 0) return;

            foreach (var deadStation in deadStations)
            {
                _logFile.AppendLine($"Dead client {deadStation.UserName}/{deadStation.ClientIp} with connectionId {deadStation.ConnectionId} and last checkout time {deadStation.LastConnectionTimestamp:T} removed.");

                var command = new LostClientConnection();
                await _eventStoreService.SendCommand(command, deadStation.UserName, deadStation.ClientIp);

                _clients.Remove(deadStation);
            }
            LogStations();
        }


        public List<DoubleAddress> GetAllDesktopClientsAddresses()
        {
            return _clients
                .Where(s => !s.IsWebClient)
                .Select(c => new DoubleAddress() { Main = new NetAddress(c.ClientIp, c.ClientAddressPort) }).ToList();
        }

        public DoubleAddress GetOneDesktopClientAddress(string clientIp)
        {
            if (clientIp == null)
                return null;
            var client = _clients.FirstOrDefault(c => c.ClientIp == clientIp && !c.IsWebClient);
            return client == null
                ? null
                : new DoubleAddress() { Main = new NetAddress(client.ClientIp, client.ClientAddressPort) };
        }

        public DoubleAddress GetClientAddressByConnectionId(string connectionId)
        {
            if (connectionId == null)
                return null;

            var client = _clients.FirstOrDefault(c => c.ConnectionId == connectionId);
            return client == null
                ? null
                : new DoubleAddress() { Main = new NetAddress(client.ClientIp, client.ClientAddressPort) };
        }

        public ClientStation GetClientByConnectionId(string connectionId)
        {
            return connectionId == null ? null : _clients.FirstOrDefault(c => c.ConnectionId == connectionId);
        }

        public bool HasAnyWebClients()
        {
            return _clients.Any(s => s.IsWebClient);
        }

        public List<string> GetWebClientsId()
        {
            return _clients.Where(c => c.IsWebClient).Select(l => l.ConnectionId).ToList();
        }

        public List<ClientStation> GetWebClients()
        {
            return _clients.Where(c => c.IsWebClient).ToList();
        }

        public ClientStation GetClientByClientIp(string clientIp)
        {
            return _clients.FirstOrDefault(c => c.ClientIp == clientIp);
        }

        public ClientStation GetStationByConnectionId(string connectionId)
        {
            return _clients.FirstOrDefault(s => s.ConnectionId == connectionId);
        }

        public void LogStations()
        {
            _logFile.EmptyLine();
            _logFile.AppendLine($"There are {_clients.Count} client(s):");
            _logFile.EmptyLine('-');
            foreach (var station in _clients)
            {
                _logFile.AppendLine($"{station.UserName}/{station.ClientIp}:{station.ClientAddressPort} with connection id {station.ConnectionId}");
            }
            _logFile.EmptyLine('-');
            _logFile.EmptyLine();
        }
    }
}
