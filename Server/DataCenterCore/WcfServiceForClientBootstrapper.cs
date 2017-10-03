using System;
using System.ServiceModel;
using Autofac;
using Autofac.Integration.Wcf;
using Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public class WcfServiceForClientBootstrapper : IDisposable
    {
        private readonly IniFile _config;
        private readonly IMyLog _log;
        private readonly IWcfServiceForClient _wcfServiceForClient;
        private readonly ILifetimeScope _container;
        private ServiceHost _host;

        public WcfServiceForClientBootstrapper(IniFile config, IMyLog log, IWcfServiceForClient wcfServiceForClient,  ILifetimeScope container)
        {
            _config = config;
            _log = log;
            _wcfServiceForClient = wcfServiceForClient;
            _container = container;
        }

        public void Start()
        {
            try
            {
                var uri = new Uri(WcfFactory.CombineUriString(@"localhost",
                    (int)TcpPorts.ServerListenToClient, @"WcfServiceForClient"));

//                _host = new ServiceHost(typeof(WcfServiceForClient), uri);
                _host = new ServiceHost(_wcfServiceForClient);
                _host.AddServiceEndpoint(typeof(IWcfServiceForClient),
                    WcfFactory.CreateDefaultNetTcpBinding(_config), uri);
                _host.AddDependencyInjectionBehavior<IWcfServiceForClient>(_container);

                _host.Open();
                _log.AppendLine("Clients listener started successfully");
            }
            catch (Exception e)
            {
                _log.AppendLine(e.Message);
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