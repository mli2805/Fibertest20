using System;
using System.ServiceModel;
using Autofac;
using Autofac.Integration.Wcf;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WcfServiceForRtuInterface;

namespace Iit.Fibertest.DataCenterCore
{
    public class WcfServiceForRtuBootstrapper : IDisposable
    {
        private readonly IniFile _config;
        private readonly IMyLog _logFile;
        private readonly ILifetimeScope _container;
        private ServiceHost _host;

        public WcfServiceForRtuBootstrapper(IniFile config, IMyLog logFile, ILifetimeScope container)
        {
            _config = config;
            _logFile = logFile;
            _container = container;
        }

        public void Start()
        {
            try
            {
                var uri = new Uri(WcfFactory.CombineUriString(@"localhost",
                    (int)TcpPorts.ServerListenToRtu, @"WcfServiceForRtu"));

                _host = new ServiceHost(typeof(WcfServiceForRtu));
                _host.AddServiceEndpoint(typeof(IWcfServiceForRtu),
                    WcfFactory.CreateDefaultNetTcpBinding(_config), uri);
                _host.AddDependencyInjectionBehavior<IWcfServiceForRtu>(_container);

                _host.Open();
                _logFile.AppendLine($"RTU listener on port {(int)TcpPorts.ServerListenToRtu} started successfully");
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