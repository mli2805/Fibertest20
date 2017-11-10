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

       
        public void StartWcfListener()
        {
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