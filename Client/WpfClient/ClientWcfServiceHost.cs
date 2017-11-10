using System;
using System.ServiceModel;
using ClientWcfServiceInterface;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public sealed class ClientWcfServiceHost : IClientWcfServiceHost
    {
        private readonly ServiceHost _wcfHost = new ServiceHost(typeof(ClientWcfService));
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        public ClientWcfServiceHost(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }

        private void ClientWcfService_MessageReceived(object e)
        {
            if (e is MonitoringResultDto)
                _logFile.AppendLine(@"Moniresult happened");
        }

        public void StartWcfListener()
        {
            ClientWcfService.ClientLog = _logFile;
            ClientWcfService.MessageReceived += ClientWcfService_MessageReceived;

            try
            {
                _wcfHost.AddServiceEndpoint(typeof(IClientWcfService), 
                    WcfFactory.CreateDefaultNetTcpBinding(_iniFile), 
                    WcfFactory.CombineUriString(@"localhost", (int)TcpPorts.ClientListenTo, @"ClientWcfService"));
                _wcfHost.Open();
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                throw;
            }
        }

    }
}