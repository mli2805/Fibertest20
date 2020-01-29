using System;
using System.ServiceModel;
using Autofac;
using Autofac.Integration.Wcf;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public class WcfServiceForWebProxyBootstrapper : IDisposable
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly ILifetimeScope _container;
        private ServiceHost _host;

        public WcfServiceForWebProxyBootstrapper(IniFile iniFile, IMyLog logFile, ILifetimeScope container)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _container = container;
        }

        public void Start()
        {
            try
            {
                var uri = new Uri(WcfFactory.CombineUriString(@"localhost",
                    (int)TcpPorts.ServerListenToWebProxy, @"WcfServiceForWebProxy"));

                _host = new ServiceHost(typeof(WcfServiceForWebProxy));
                _host.AddServiceEndpoint(typeof(IWcfServiceForWebProxy),
                    WcfFactory.CreateDefaultNetTcpBinding(_iniFile), uri);
                _host.AddDependencyInjectionBehavior<IWcfServiceForWebProxy>(_container);

                _host.Open();
                _logFile.AppendLine($"WebProxy (Web Clients) listener on port {(int)TcpPorts.ServerListenToWebProxy} started successfully");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                throw;
            }
        }

        public void Dispose()
        {
            _host?.Close();
            _host = null;
        }
    }
}