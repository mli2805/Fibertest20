using System;
using System.ServiceModel;
using ClientWcfServiceInterface;
using Iit.Fibertest.ClientWcfServiceLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public sealed class ClientWcfServiceHost : IClientWcfServiceHost
    {
//        private readonly ServiceHost _wcfHost = new ServiceHost(typeof(ClientWcfService));
        private ServiceHost _wcfHost;
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly ClientWcfService _clientWcfService;

        public ClientWcfServiceHost(IniFile iniFile, IMyLog logFile, ClientWcfService clientWcfService)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _clientWcfService = clientWcfService;
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
                var uri = new Uri(WcfFactory.CombineUriString(@"localhost", (int)TcpPorts.ClientListenTo, @"ClientWcfService"));
                _wcfHost = new ServiceHost(_clientWcfService);
                _wcfHost.AddServiceEndpoint(typeof(IClientWcfService), WcfFactory.CreateDefaultNetTcpBinding(_iniFile), uri );
                _wcfHost.Open();
                _logFile.AppendLine(@"Datacenter listener started successfully");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                throw;
            }
        }

    }
}