using System;
using System.ServiceModel;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.RtuWpfExample.ServiceReference1;

namespace Iit.Fibertest.RtuWpfExample
{
    public class WcfClientViewModel : Screen
    {
        private RtuWcfServiceClient _rtuWcfServiceClient;

        public void WcfTest()
        {
                        StartRtuWcfServiceClient("192.168.96.179");
//            _rtuWcfServiceClient = new RtuWcfServiceClient();

            MessageBox.Show(_rtuWcfServiceClient.GetData(123), @"My WCF Service");
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

                netTcpBinding.MaxBufferSize = 4096000; //4M
                _rtuWcfServiceClient = new RtuWcfServiceClient(netTcpBinding, new EndpointAddress(new Uri(uriString)));
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
                throw;
            }
        }
    }
}
