using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class ClientsCollection
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly EventStoreService _eventStoreService;
        private readonly List<ClientStation> _clients = new List<ClientStation>();

        public ClientsCollection(IniFile iniFile, IMyLog logFile, Model writeModel,
            CurrentDatacenterParameters currentDatacenterParameters, EventStoreService eventStoreService)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
            _currentDatacenterParameters = currentDatacenterParameters;
            _eventStoreService = eventStoreService;
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == dto.UserName && UserExt.FlipFlop(u.EncodedPassword) == dto.Password);
            if (user == null)
                return new ClientRegisteredDto { ReturnCode = ReturnCode.NoSuchUserOrWrongPassword };

            var station = _clients.FirstOrDefault(s => s.ClientIp == dto.ClientIp);
            if (station != null && !(station.IsWebClient ^ dto.IsWebClient))
            {
                _clients.Remove(station);
            }

            var hasRight = CheckUsersRights(dto, user);
            if (hasRight != null)
                return hasRight;

            var licenseCheckResult = CheckLicense(dto);
            if (licenseCheckResult != null)
            {
                _logFile.AppendLine(licenseCheckResult.ReturnCode.GetLocalizedString());
                return licenseCheckResult;
            }
            return await RegisterClientStation(dto, user);
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
                if (_clients.Count(c => c.UserRole == Role.Superclient) >= _writeModel.License.SuperClientStationCount.Value
                    && _clients.All(s => s.ClientIp != dto.ClientIp))
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.SuperClientsCountExceeded };
                if (_writeModel.License.SuperClientStationCount.ValidUntil < DateTime.Today)
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.SuperClientsCountLicenseExpired };
            }
            else if (dto.IsWebClient)
            {
                if (_clients.Count(c => c.IsWebClient) >= _writeModel.License.WebClientCount.Value
                    && _clients.All(s => s.ClientIp != dto.ClientIp))
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.WebClientsCountExceeded };
                if (_writeModel.License.WebClientCount.ValidUntil < DateTime.Today)
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.WebClientsCountLicenseExpired };
            }
            else
            {
                if (_clients.Count(c => c.IsDesktopClient) >= _writeModel.License.ClientStationCount.Value
                    && _clients.All(s => s.ClientIp != dto.ClientIp))
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.ClientsCountExceeded };
                if (_writeModel.License.ClientStationCount.ValidUntil < DateTime.Today)
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.ClientsCountLicenseExpired };
            }
            return null;
        }

        private async Task<ClientRegisteredDto> RegisterClientStation(RegisterClientDto dto, User user)
        {
            var theSame = _clients.FirstOrDefault(s => s.UserId == user.UserId && s.ClientIp != dto.ClientIp);
            if (theSame != null)
            {
                _logFile.AppendLine($"The same user {dto.UserName} registered from device {theSame.ClientIp}");
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.ThisUserRegisteredFromAnotherDevice };
            }

            var station = _clients.FirstOrDefault(s => s.ClientIp == dto.ClientIp);
            if (station != null)
                ReRegister(dto, station, user);
            else
                RegisterNew(dto, user);

            _logFile.AppendLine($"There are {_clients.Count()} client(s)");
            return await FillInSuccessfulResult(user);
        }

        private void ReRegister(RegisterClientDto dto, ClientStation station, User user)
        {
            station.UserId = user.UserId;
            station.UserName = dto.UserName;
            station.LastConnectionTimestamp = DateTime.Now;
            _logFile.AppendLine($"Client {dto.UserName} from {dto.ClientIp} was registered already. Re-registered.");
        }

        private void RegisterNew(RegisterClientDto dto, User user)
        {
            var station = new ClientStation()
            {
                UserId = user.UserId,
                UserName = dto.UserName,
                UserRole = user.Role,
                ClientIp = dto.Addresses.Main.GetAddress(),
                ClientAddressPort = dto.Addresses.Main.Port,
                LastConnectionTimestamp = DateTime.Now,

                IsUnderSuperClient = dto.IsUnderSuperClient,
                IsWebClient = dto.IsWebClient,
                IsDesktopClient = !dto.IsUnderSuperClient && !dto.IsWebClient,
            };
            _clients.Add(station);
            _logFile.AppendLine($"Client {dto.UserName} from {dto.ClientIp} registered");
        }

#pragma warning disable 1998
        private async Task<ClientRegisteredDto> FillInSuccessfulResult(User user)
#pragma warning restore 1998
        {
            var result = new ClientRegisteredDto();
            result.UserId = user.UserId;
            result.Role = user.Role;
            var zone = _writeModel.Zones.First(z => z.ZoneId == user.ZoneId);
            result.ZoneId = zone.ZoneId;
            result.ZoneTitle = zone.Title;
            result.DatacenterVersion = _currentDatacenterParameters.DatacenterVersion;
            result.ReturnCode = ReturnCode.ClientRegisteredSuccessfully;
            result.IsWithoutMapMode = _iniFile.Read(IniSection.Server, IniKey.IsWithoutMapMode, false);
            result.Smtp = _currentDatacenterParameters.Smtp;
            result.GsmModemComPort = _currentDatacenterParameters.GsmModemComPort;
            result.Snmp = _currentDatacenterParameters.Snmp;
            return result;
        }

        public void RegisterHeartbeat(string clientIp)
        {
            var client = _clients.FirstOrDefault(c => c.ClientIp == clientIp);
            if (client != null)
                client.LastConnectionTimestamp = DateTime.Now;
        }

        // if user just closed the browser tab instead of logging out
        // WebApi has not got user's name and put it as "onSignalRDisconnected"
        public void UnregisterClientAsync(UnRegisterClientDto dto)
        {
            _logFile.AppendLine($"dto: username: {dto.Username}, clientIp: {dto.ClientIp}");
            var station = _clients.FirstOrDefault(s => s.ClientIp == dto.ClientIp &&
                               (s.UserName == dto.Username || (dto.Username == "onSignalRDisconnected" && s.IsWebClient)));
            if (station != null)
            {
                _clients.Remove(station);
                _logFile.AppendLine($"Client {dto.Username} from {dto.ClientIp} unregistered.");
            }
            else
                _logFile.AppendLine($"There is no client station with address {dto.ClientIp}");

            _logFile.AppendLine($"There are {_clients.Count()} client(s) now");
            int i = 0;
            foreach (var clientStation in _clients)
            {
                _logFile.AppendLine($"{++i}. {clientStation.UserName} from {clientStation.ClientIp} as webclient = {clientStation.IsWebClient}");
            }
        }

        public async void CleanDeadClients(TimeSpan timeSpan)
        {
            DateTime noLaterThan = DateTime.Now - timeSpan;
            var deadStations = _clients.Where(s => s.LastConnectionTimestamp < noLaterThan && !s.IsWebClient).ToList();
            if (deadStations.Count == 0) return;

            foreach (var deadStation in deadStations)
            {
                _logFile.AppendLine($"Dead client {deadStation.UserName} from {deadStation.ClientIp} removed.");

                var command = new LostClientConnection();
                await _eventStoreService.SendCommand(command, deadStation.UserName, deadStation.ClientIp);

                _clients.Remove(deadStation);
            }
            _logFile.AppendLine($"There are {_clients.Count()} client(s)");
        }


        public List<DoubleAddress> GetDesktopClientsAddresses(string clientIp = null)
        {
            if (clientIp == null)
                return _clients
                    .Where(s => !s.IsWebClient)
                    .Select(c => new DoubleAddress() { Main = new NetAddress(c.ClientIp, c.ClientAddressPort) }).ToList();
            var client = _clients.FirstOrDefault(c => c.ClientIp == clientIp);
            return client == null
                ? null
                : new List<DoubleAddress>() { new DoubleAddress() { Main = new NetAddress(client.ClientIp, client.ClientAddressPort) } };
        }

        public List<DoubleAddress> GetWebClientsAddresses(string clientIp = null)
        {
            if (clientIp == null)
                return _clients
                    .Where(s => s.IsWebClient)
                    .Select(c => new DoubleAddress() { Main = new NetAddress(c.ClientIp, c.ClientAddressPort) }).ToList();
            var client = _clients.FirstOrDefault(c => c.ClientIp == clientIp);
            return client == null
                ? null
                : new List<DoubleAddress>() { new DoubleAddress() { Main = new NetAddress(client.ClientIp, client.ClientAddressPort) } };
        }

        public bool HasAnyWebClients()
        {
            return _clients.Any(s => s.IsWebClient);
        }

        public ClientStation GetClientStation(string clientIp)
        {
            return _clients.FirstOrDefault(c => c.ClientIp == clientIp);
        }
    }
}
