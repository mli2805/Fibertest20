using System;
using System.ServiceModel;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.RtuWpfExample.ServiceReference1;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.RtuWpfExample
{
    public class WcfClientViewModel : Screen
    {
        private readonly Logger35 _rtuLogger;
        private RtuWcfServiceClient _rtuWcfServiceClient;
        private string _rtuServiceIp;

        public string RtuServiceIp
        {
            get { return _rtuServiceIp; }
            set
            {
                if (value == _rtuServiceIp) return;
                _rtuServiceIp = value;
                NotifyOfPropertyChange();
            }
        }

        public WcfClientViewModel(IniFile iniFile35, Logger35 rtuLogger)
        {
            _rtuLogger = rtuLogger;
            RtuServiceIp = iniFile35.Read(IniSection.General, IniKey.RtuServiceIp, @"192.168.96.53");
        }

        public void StartMonitoring()
        {
            _rtuWcfServiceClient = CreateRtuWcfServiceClient(RtuServiceIp);
            if (_rtuWcfServiceClient == null) return;

            try
            {
                _rtuWcfServiceClient.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"My WCF Service");
                return;
            }
            _rtuWcfServiceClient.StartMonitoring();
        }

        public void StopMonitoring()
        {
            _rtuWcfServiceClient = CreateRtuWcfServiceClient(RtuServiceIp);
            if (_rtuWcfServiceClient == null) return;

            try
            {
                _rtuWcfServiceClient.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, @"My WCF Service");
                return;
            }
            _rtuWcfServiceClient.StopMonitoring();
        }

        private RtuWcfServiceClient CreateRtuWcfServiceClient(string address)
        {
            try
            {
                var uriString = @"net.tcp://" + address + @":11842/RtuWcfService";
                var netTcpBinding = new NetTcpBinding();
                netTcpBinding.Security.Mode = SecurityMode.None;
                netTcpBinding.ReceiveTimeout = new TimeSpan(0, 15, 0);
                netTcpBinding.SendTimeout = new TimeSpan(0, 15, 0);
                netTcpBinding.OpenTimeout = new TimeSpan(0, 1, 0);

                netTcpBinding.MaxBufferSize = 4096000; //4M
                var rtuWcfServiceClient = new RtuWcfServiceClient(netTcpBinding, new EndpointAddress(new Uri(uriString)));
                _rtuLogger.AppendLine($@"Wcf client to {RtuServiceIp} created");
                return rtuWcfServiceClient;
            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine(e.Message);
                MessageBox.Show(e.Message, @"My WCF Service");
                return null;
            }
        }
    }
}
