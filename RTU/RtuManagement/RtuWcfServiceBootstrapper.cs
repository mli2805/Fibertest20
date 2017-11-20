using System;
using System.ServiceModel;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.RtuManagement
{
    public class RtuWcfServiceBootstrapper : IDisposable
    {
        private readonly IniFile _serviceIni;
        private readonly IMyLog _serviceLog;
        private readonly IRtuWcfService _rtuWcfService;
        private ServiceHost _host;

        public RtuWcfServiceBootstrapper(IniFile config, IMyLog log, IRtuWcfService rtuWcfService)
        {
            _serviceIni = config;
            _serviceLog = log;
            _rtuWcfService = rtuWcfService;
        }

        public void Start()
        {
            try
            {
                var uri = new Uri(WcfFactory.CombineUriString(@"localhost", (int)TcpPorts.RtuListenTo, @"RtuWcfService"));
                var rtuId = Guid.Parse(_serviceIni.Read(IniSection.Server, IniKey.RtuGuid, Guid.Empty.ToString()));

                _host = new ServiceHost(_rtuWcfService);
                _host.AddServiceEndpoint(typeof(IRtuWcfService),
                    WcfFactory.CreateDefaultNetTcpBinding(_serviceIni), uri);
                _host.Open();
                _serviceLog.AppendLine($"RTU {rtuId.First6()} is listening to DataCenter now.");
            }
            catch (Exception e)
            {
                _serviceLog.AppendLine(e.Message);
                var enabled = _serviceIni.Read(IniSection.Recovering, IniKey.RebootSystemEnabled, false);
                if (enabled)
                {
                    var delay = _serviceIni.Read(IniSection.Recovering, IniKey.RebootSystemDelay, 60);
                    _serviceLog.AppendLine("Recovery procedure: Reboot system.");
                    RestoreFunctions.RebootSystem(_serviceLog, delay);
                    Thread.Sleep(TimeSpan.FromSeconds(delay + 5));
                }
                else
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