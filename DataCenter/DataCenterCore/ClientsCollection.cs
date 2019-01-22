using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private readonly EventStoreService _eventStoreService;
        private readonly List<ClientStation> _clients = new List<ClientStation>();

        public ClientsCollection(IniFile iniFile, IMyLog logFile, Model writeModel, EventStoreService eventStoreService)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
            _eventStoreService = eventStoreService;
        }

        public ClientRegisteredDto RegisterClientAsync(RegisterClientDto dto)
        {
            var user = _writeModel.Users.FirstOrDefault(u => u.Title == dto.UserName && UserExt.FlipFlop(u.EncodedPassword) == dto.Password);
            return user == null
                ? new ClientRegisteredDto { ReturnCode = ReturnCode.NoSuchUserOrWrongPassword }
                : HasRight(dto, user);
        }

        private ClientRegisteredDto HasRight(RegisterClientDto dto, User user)
        {
            if (!dto.IsUnderSuperClient)
            {
                if (user.Role >= Role.Superclient)
                    return new ClientRegisteredDto() {ReturnCode = ReturnCode.UserHasNoRightsToStartClient};
            }
            else
            {
                if  (user.Role != Role.Superclient && user.Role != Role.Developer)
                    return new ClientRegisteredDto() {ReturnCode = ReturnCode.UserHasNoRightsToStartSuperClient};
            }
            return RegisterClientStation(dto, user);
        }

        private ClientRegisteredDto RegisterClientStation(RegisterClientDto dto, User user)
        {
            var licenseCheckResult = CheckLicense(user);
            if (licenseCheckResult != null) return licenseCheckResult;

            if (_clients.Any(s => s.UserId == user.UserId && s.ClientGuid != dto.ClientId))
            {
                _logFile.AppendLine($"User {dto.UserName} registered on another PC");
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.ThisUserRegisteredOnAnotherPc };
            }

            var station = _clients.FirstOrDefault(s => s.ClientGuid == dto.ClientId);
            if (station != null)
                ReRegister(dto, station, user);
            else
                RegisterNew(dto, user);

            _logFile.AppendLine($"There are {_clients.Count()} client(s)");
            return FillInSuccessfulResult(user);
        }

        private ClientRegisteredDto CheckLicense(User user)
        {
            if (user.Role != Role.Superclient && _clients.Count(c => c.UserRole != Role.Superclient) >=
                _writeModel.License.ClientStationCount.Value)
            {
                _logFile.AppendLine("Exceeded the number of clients registered simultaneously");
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.ClientsCountExceeded };
            }

            if (user.Role == Role.Superclient && _clients.Count(c => c.UserRole == Role.Superclient) >= _writeModel.License.SuperClientStationCount.Value)
            {
                _logFile.AppendLine("Exceeded the number of super-clients registered simultaneously");
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.SuperClientsCountExceeded };
            }

            if (user.Role != Role.Superclient && _writeModel.License.ClientStationCount.ValidUntil < DateTime.Today)
            {
                _logFile.AppendLine("Clients count license expired");
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.ClientsCountLicenseExpired };
            }

            if (user.Role == Role.Superclient && _writeModel.License.SuperClientStationCount.ValidUntil < DateTime.Today)
            {
                _logFile.AppendLine("Super-clients count license expired");
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.SuperClientsCountLicenseExpired };
            }

            return null;
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
                ClientGuid = dto.ClientId,
                ClientAddress = dto.Addresses.Main.GetAddress(),
                ClientAddressPort = dto.Addresses.Main.Port,
                LastConnectionTimestamp = DateTime.Now,
            };
            _clients.Add(station);
            _logFile.AppendLine($"Client {dto.UserName} from {dto.ClientIp} registered");
        }

        private ClientRegisteredDto FillInSuccessfulResult(User user)
        {
            var result = new ClientRegisteredDto();
            result.UserId = user.UserId;
            result.Role = user.Role;
            var zone = _writeModel.Zones.First(z => z.ZoneId == user.ZoneId);
            result.ZoneId = zone.ZoneId;
            result.ZoneTitle = zone.Title;
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            result.DatacenterVersion = fvi.FileVersion;
            result.ReturnCode = ReturnCode.ClientRegisteredSuccessfully;
            result.IsInGisVisibleMode = _iniFile.Read(IniSection.Server, IniKey.IsInGisVisibleMode, true);

            result.Smtp = new SmtpSettingsDto()
            {
                SmptHost = _iniFile.Read(IniSection.Smtp, IniKey.SmtpHost, ""),
                SmptPort = _iniFile.Read(IniSection.Smtp, IniKey.SmtpPort, 0),
                MailFrom = _iniFile.Read(IniSection.Smtp, IniKey.MailFrom, ""),
                MailFromPassword = _iniFile.Read(IniSection.Smtp, IniKey.MailFromPassword, ""),
                SmtpTimeoutMs = _iniFile.Read(IniSection.Smtp, IniKey.SmtpTimeoutMs, 0),
            };

            result.GsmModemComPort = _iniFile.Read(IniSection.Broadcast, IniKey.GsmModemComPort, 0);
            return result;
        }

        public void RegisterHeartbeat(Guid clientGuid)
        {
            var client = _clients.FirstOrDefault(c => c.ClientGuid == clientGuid);
            if (client != null)
                client.LastConnectionTimestamp = DateTime.Now;
        }

        public void UnregisterClientAsync(UnRegisterClientDto dto)
        {
            var station = _clients.FirstOrDefault(s => s.ClientGuid == dto.ClientId);
            if (station == null)
            {
                _logFile.AppendLine("There is no client station with such guid");
                return;
            }

            _clients.Remove(station);
            _logFile.AppendLine($"Client unregistered. There are {_clients.Count()} client(s) now");
        }

        public async void CleanDeadClients(TimeSpan timeSpan)
        {
            DateTime noLaterThan = DateTime.Now - timeSpan;
            var deadStations = _clients.Where(s => s.LastConnectionTimestamp < noLaterThan).ToList();
            if (deadStations.Count == 0) return;

            foreach (var deadStation in deadStations)
            {
                _logFile.AppendLine($"Dead client {deadStation.UserName} from {deadStation.ClientAddress} removed.");

                var command = new LostClientConnection();
                await _eventStoreService.SendCommand(command, deadStation.UserName, deadStation.ClientAddress);

                _clients.Remove(deadStation);
            }
            _logFile.AppendLine($"There are {_clients.Count()} client(s)");
        }

        public List<DoubleAddress> GetClientsAddresses(Guid? clientId = null)
        {
            if (clientId == null)
                return _clients.Select(c => new DoubleAddress() { Main = new NetAddress(c.ClientAddress, c.ClientAddressPort) }).ToList();
            var client = _clients.FirstOrDefault(c => c.ClientGuid == clientId);
            return client == null
                ? null
                : new List<DoubleAddress>() { new DoubleAddress() { Main = new NetAddress(client.ClientAddress, client.ClientAddressPort) } };
        }
    }
}
