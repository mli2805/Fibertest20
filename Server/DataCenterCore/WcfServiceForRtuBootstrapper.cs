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
        private readonly IMyLog _log;
        private readonly IWcfServiceForRtu _wcfServiceForRtu;
        private readonly ILifetimeScope _container;
        private ServiceHost _host;

        public WcfServiceForRtuBootstrapper(IniFile config, IMyLog log, IWcfServiceForRtu wcfServiceForRtu,  ILifetimeScope container)
        {
            _config = config;
            _log = log;
            _wcfServiceForRtu = wcfServiceForRtu;
            _container = container;
        }

        public void Start()
        {
            try
            {
                var uri = new Uri(WcfFactory.CombineUriString(@"localhost",
                    (int)TcpPorts.ServerListenToRtu, @"WcfServiceForRtu"));

                _host = new ServiceHost(_wcfServiceForRtu);
                _host.AddServiceEndpoint(typeof(IWcfServiceForRtu),
                    WcfFactory.CreateDefaultNetTcpBinding(_config), uri);
                _host.AddDependencyInjectionBehavior<IWcfServiceForRtu>(_container);

                _host.Open();
                _log.AppendLine("Rtus listener started successfully");
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