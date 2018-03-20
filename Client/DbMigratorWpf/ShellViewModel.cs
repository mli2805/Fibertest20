using System;
using Caliburn.Micro;
using System.ServiceModel;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace DbMigratorWpf
{
    public class ShellViewModel : PropertyChangedBase, IShell
    {
        private readonly IMyLog _logFile;
        private static IniFile _iniFile;

        private Guid _clientId;
        private Wcf _wcf;

        public ShellViewModel()
        {
            _iniFile = new IniFile();
            _iniFile.AssignFile(@"migrator.ini");

            _logFile = new LogFile(_iniFile).AssignFile(@"rtu.log");
            _wcf = new Wcf(_iniFile);
            _clientId = Guid.NewGuid();
        }

        public async void BtnSendRegister()
        {
            ChannelFactory<IWcfServiceForClient> ddd = _wcf.GetChannelFactory();
            var channel = ddd.CreateChannel();
            var result = await channel.RegisterClientAsync(
                new RegisterClientDto()
                {
                    UserName = "developer", Password = "developer", ClientId = _clientId,
                    Addresses = new DoubleAddress() { Main = new NetAddress("192.168.96.21", TcpPorts.ClientListenTo)}
                });
            ddd.Close();
        }

        public async void BtnSendHeartbeat()
        {
            ChannelFactory<IWcfServiceForClient> ddd = _wcf.GetChannelFactory();
            var channel = ddd.CreateChannel();
            var result = await channel.RegisterClientAsync(new RegisterClientDto() { IsHeartbeat = true });
            ddd.Close();
        }
    }

    public class Wcf
    {
        private readonly IniFile _iniFile;

        public Wcf(IniFile iniFile)
        {
            _iniFile = iniFile;
        }

        public ChannelFactory<IWcfServiceForClient> GetChannelFactory()
        {

            var myClient = new MyClient<IWcfServiceForClient>(
                GetNetTcpBinding(),
//                new EndpointAddress(new Uri(CombineUriString("192.168.96.21", 11840, @"WcfServiceForClient"))));
                new EndpointAddress(new Uri(CombineUriString("172.16.4.105", 11840, @"WcfServiceForClient"))));
            return myClient.ChannelFactory;
        }

        public string CombineUriString(string address, int port, string wcfServiceName)
        {
            return @"net.tcp://" + address + @":" + port + @"/" + wcfServiceName;
        }
        private NetTcpBinding GetNetTcpBinding()
        {
            return new NetTcpBinding
            {
                Security = { Mode = SecurityMode.None },
                OpenTimeout = TimeSpan.FromSeconds(20),

                ReceiveTimeout = TimeSpan.FromSeconds(_iniFile.Read(IniSection.NetTcpBinding, IniKey.ReadTimeout, 60)),
                SendTimeout = TimeSpan.FromSeconds(_iniFile.Read(IniSection.NetTcpBinding, IniKey.SendTimeout, 60)),
                ReaderQuotas = { MaxArrayLength = 40960000 },
                MaxBufferSize = 40960000, //4M
                MaxReceivedMessageSize = 40960000,
            };
        }
    }
}