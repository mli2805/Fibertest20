using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class ClientHeartbeat
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly IWcfServiceForClient _c2DWcfManager;

        private NetAddress _clientAddresses;
        private TimeSpan _clientHeartbeatRate;

        public ClientHeartbeat(IniFile iniFile, IMyLog logFile, IWcfServiceForClient c2DWcfManager)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _c2DWcfManager = c2DWcfManager;
        }

        public void Start()
        {
            _clientHeartbeatRate =
                TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.ClientHeartbeatRate, 5));
            _logFile.AppendLine(@"Heartbeat started");

            _clientAddresses = _iniFile.Read(IniSection.ClientLocalAddress, (int)TcpPorts.ClientListenTo);

            var heartbeatThread = new Thread(Beat) { IsBackground = true };
            heartbeatThread.Start();
        }

        // ReSharper disable once FunctionNeverReturns 
        private void Beat()
        {
            while (true)
            {
                RegisterHeartbeatAsync().Wait();
                Thread.Sleep(_clientHeartbeatRate);
            }
        }

        private async Task<ClientRegisteredDto> RegisterHeartbeatAsync()
        {
            var result = await _c2DWcfManager.RegisterClientAsync(
                new RegisterClientDto()
                {
                    Addresses = new DoubleAddress() { Main = _clientAddresses, HasReserveAddress = false },
                    IsHeartbeat = true,
                });

            if (result.ReturnCode != ReturnCode.ClientRegisteredSuccessfully)
            {
                _logFile.AppendLine(result.ReturnCode.ToString());
                MessageBox.Show(result.ReturnCode.GetLocalizedString(), Resources.SID_Error);
            }
            return result;
        }

    }
}
