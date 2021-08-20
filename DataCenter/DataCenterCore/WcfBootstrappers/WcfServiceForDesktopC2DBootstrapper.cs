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
    public class WcfServiceForDesktopC2DBootstrapper : IDisposable
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly ILifetimeScope _container;
        private ServiceHost _host;

        public WcfServiceForDesktopC2DBootstrapper(IniFile iniFile, IMyLog logFile, ILifetimeScope container)
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
                    (int)TcpPorts.ServerListenToDesktopClient, @"WcfServiceForDesktopC2D"));

                _host = new ServiceHost(typeof(WcfServiceDesktopC2D));
                _host.AddServiceEndpoint(typeof(IWcfServiceDesktopC2D),
                    WcfFactory.CreateDefaultNetTcpBinding(_iniFile), uri);
                _host.AddDependencyInjectionBehavior<IWcfServiceDesktopC2D>(_container);

                var behavior = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
                if (behavior == null)
                    _host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
                else if (!behavior.IncludeExceptionDetailInFaults)
                    behavior.IncludeExceptionDetailInFaults = true;

                _host.Open();
                _logFile.AppendLine($"Clients (desktop) listener on port {(int)TcpPorts.ServerListenToDesktopClient} started successfully");
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