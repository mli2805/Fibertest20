using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Autofac;
using Autofac.Integration.Wcf;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.DataCenterCore
{
    public class WcfServiceForClientBootstrapper : IDisposable
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly ILifetimeScope _container;
        private ServiceHost _host;

        public WcfServiceForClientBootstrapper(IniFile iniFile, IMyLog logFile, ILifetimeScope container)
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
                    (int)TcpPorts.ServerListenToClient, @"WcfServiceForClient"));

                _host = new ServiceHost(typeof(WcfServiceForClient));
                _host.AddServiceEndpoint(typeof(IWcfServiceForClient),
                    WcfFactory.CreateDefaultNetTcpBinding(_iniFile), uri);
                _host.AddDependencyInjectionBehavior<IWcfServiceForClient>(_container);

                var behavior = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
                if (behavior == null)
                    _host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
                else if (!behavior.IncludeExceptionDetailInFaults)
                    behavior.IncludeExceptionDetailInFaults = true;

                _host.Open();
                _logFile.AppendLine("Clients listener started successfully");
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