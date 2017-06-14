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
        private readonly IniFile _iniFile35;
        private readonly Logger35 _rtuLogger;
        private RtuWcfServiceClient _rtuWcfServiceClient;

        public WcfClientViewModel(IniFile iniFile35, Logger35 rtuLogger)
        {
            _iniFile35 = iniFile35;
            _rtuLogger = rtuLogger;
        }

        public void WcfTest()
        {
            var rtuAddress = _iniFile35.Read(IniSection.General, IniKey.OtdrIp, @"192.168.96.53");
            StartRtuWcfServiceClient(rtuAddress);

            MessageBox.Show(_rtuWcfServiceClient.GetData(456), @"My WCF Service");
            _rtuWcfServiceClient.Close();
        }


        private void StartRtuWcfServiceClient(string address)
        {
            try
            {
                var uriString = @"net.tcp://" + address + @":8523/RtuWcfService";
                var netTcpBinding = new NetTcpBinding();
                netTcpBinding.Security.Mode = SecurityMode.None;
                netTcpBinding.ReceiveTimeout = new TimeSpan(0, 15, 0);
                netTcpBinding.SendTimeout = new TimeSpan(0, 15, 0);
                netTcpBinding.OpenTimeout = new TimeSpan(0,1,0);

                netTcpBinding.MaxBufferSize = 4096000; //4M
                _rtuWcfServiceClient = new RtuWcfServiceClient(netTcpBinding, new EndpointAddress(new Uri(uriString)));
                _rtuLogger.AppendLine("Wcf connection established");
            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine("Can't establish wcf connection");
                _rtuLogger.AppendLine(e.Message);
                throw;
            }
        }
    }
}
