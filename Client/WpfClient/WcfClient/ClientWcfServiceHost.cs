using System;
using System.ServiceModel;
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
        private readonly WcfServiceInClient _wcfServiceInClient;

        public ClientWcfServiceHost(IniFile iniFile, IMyLog logFile, WcfServiceInClient wcfServiceInClient)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _wcfServiceInClient = wcfServiceInClient;
        }

       
        public void StartWcfListener()
        {
            try
            {
                var clientTcpPort = _iniFile.Read(IniSection.ClientLocalAddress, IniKey.TcpPort, (int)TcpPorts.ClientListenTo);
                var uri = new Uri(WcfFactory.CombineUriString(@"localhost", clientTcpPort, @"ClientWcfService"));
                _wcfHost = new ServiceHost(_wcfServiceInClient);
                _wcfHost.AddServiceEndpoint(typeof(IWcfServiceInClient), WcfFactory.CreateDefaultNetTcpBinding(_iniFile), uri );
                _wcfHost.Open();
                _logFile.AppendLine(@"DataCenter listener started successfully");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                throw;
            }
        }

    }
}