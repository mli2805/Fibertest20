using System;
using System.ServiceModel;
using System.ServiceModel.Description;
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
                    (int)TcpPorts.ServerListenToWebClient, @"WcfServiceForWebProxy"));

                _host = new ServiceHost(typeof(WcfServiceWebC2D));
                _host.AddServiceEndpoint(typeof(IWcfServiceWebC2D),
                    WcfFactory.CreateDefaultNetTcpBinding(_iniFile), uri);
                _host.AddDependencyInjectionBehavior<IWcfServiceWebC2D>(_container);

                var behavior = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
                if (behavior == null)
                    _host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
                else if (!behavior.IncludeExceptionDetailInFaults)
                    behavior.IncludeExceptionDetailInFaults = true;

                _host.Open();
                _logFile.AppendLine($"WebProxy (Web Clients) listener on port {(int)TcpPorts.ServerListenToWebClient} started successfully");
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