using System.Linq;
using System.Threading;
using Dto;
using Iit.Fibertest.Utils35;
using WcfConnections;

namespace DataCenterCore
{
    public partial class DcManager
    {
        private void WcfServiceForClient_MessageReceived(object msg)
        {
            var dto = msg as RegisterClientDto;
            if (dto != null)
            {
                RegisterClient(dto.ClientAddress);
                return;
            }
            var dto2 = msg as UnRegisterClientDto;
            if (dto2 != null)
            {
                UnRegisterClient(dto2.ClientAddress);
                return;
            }
            var dto3 = msg as CheckRtuConnectionDto;
            if (dto3 != null)
            {
                CheckRtuConnectionThread(dto3);
                return;
            }
            var dto4 = msg as InitializeRtuDto;
            if (dto4 != null)
            {
                InitializeRtu(dto4);
                return;
            }
            var dto5 = msg as StartMonitoringDto;
            if (dto5 != null)
            {
                StartMonitoring(dto5);
                return;
            }
            var dto6 = msg as StopMonitoringDto;
            if (dto6 != null)
            {
                StopMonitoring(dto6);
                return;
            }
            var dto7 = msg as AssignBaseRefDto;
            if (dto7 != null)
            {
                AssignBaseRef(dto7);
                return;
            }
            var dto8 = msg as ApplyMonitoringSettingsDto;
            if (dto8 != null)
            {
                ApplyMonitoringSettings(dto8);
                return;
            }
        }

        private void RegisterClient(string address)
        {
            _dcLog.AppendLine($"Client {address} registered");
            lock (_clientStationsLockObj)
            {
                if (_clientStations.All(c => c.Ip != address))
                    _clientStations.Add(new ClientStation() { Ip = address });
            }
        }

        private void UnRegisterClient(string address)
        {
            _dcLog.AppendLine($"Client {address} exited");
            lock (_clientStationsLockObj)
            {
                _clientStations.RemoveAll(c => c.Ip == address);
            }
        }

        private void CheckRtuConnectionThread(CheckRtuConnectionDto dto)
        {
            Thread thread = new Thread(CheckRtuConnection);
            thread.Start(dto);
        }

        private void CheckRtuConnection(object param)
        {
            var dto = param as CheckRtuConnectionDto;
            if (dto == null)
                return;

            var result = new RtuConnectionCheckedDto() { RtuId = dto.RtuId };
            var address = dto.IsAddressSetAsIp ? dto.Ip4Address : dto.HostName;
            var rtuConnection = new WcfFactory(address, _coreIni, _dcLog).CreateRtuConnection();
            result.IsRtuManagerAlive = rtuConnection != null && rtuConnection.IsRtuInitialized();

            if (!result.IsRtuManagerAlive)
                result.IsPingSuccessful = Pinger.Ping(dto.IsAddressSetAsIp ? dto.Ip4Address : dto.HostName);

            new D2CWcfManager(dto.ClientAddress, _coreIni, _dcLog).ConfirmRtuConnectionChecked(result);
        }

        private bool InitializeRtu(InitializeRtuDto rtu)
        {
            var rtuConnection = new WcfFactory(rtu.RtuIpAddress, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return false;
            rtuConnection.Initialize(rtu);

            _dcLog.AppendLine($"Transfered command to initialize RTU {rtu.RtuId} with ip={rtu.RtuIpAddress}");
            return true;
        }

        private bool StartMonitoring(StartMonitoringDto dto)
        {
            var rtuConnection = new WcfFactory(dto.RtuAddress, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return false;

            rtuConnection.StartMonitoring();
            _dcLog.AppendLine($"Transfered command to start monitoring for rtu with ip={dto.RtuAddress}");
            return true;
        }

        private bool StopMonitoring(StopMonitoringDto dto)
        {
            var rtuConnection = new WcfFactory(dto.RtuAddress, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return false;

            rtuConnection.StopMonitoring();
            _dcLog.AppendLine($"Transfered command to stop monitoring for rtu with ip={dto.RtuAddress}");
            return true;
        }

        private bool AssignBaseRef(AssignBaseRefDto baseRef)
        {
            var rtuConnection = new WcfFactory(baseRef.RtuIpAddress, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return false;

            rtuConnection.AssignBaseRef(baseRef);
            _dcLog.AppendLine($"Transfered command to assign base ref to rtu with ip={baseRef.RtuIpAddress}");
            return true;
        }

        private bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings)
        {
            var rtuConnection = new WcfFactory(settings.RtuIpAddress, _coreIni, _dcLog).CreateRtuConnection();
            if (rtuConnection == null)
                return false;

            rtuConnection.ApplyMonitoringSettings(settings);
            _dcLog.AppendLine($"Transfered command to apply monitoring settings for rtu with ip={settings.RtuIpAddress}");
            return true;
        }

    }
}
