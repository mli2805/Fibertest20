using System;
using System.Windows;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class NetAddressTestViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly Guid _clientId;
        private readonly NetAddress _serverAddress;
        private bool _result;
        public NetAddressInputViewModel NetAddressInputViewModel { get; set; }


        public bool Result
        {
            get { return _result; }
            set
            {
                if (value == _result) return;
                _result = value;
                NotifyOfPropertyChange();
            }
        }

        public NetAddressTestViewModel(NetAddress addressForTesting, IniFile iniFile, IMyLog logFile, Guid clientId, NetAddress serverAddress = null)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _clientId = clientId;
            _serverAddress = serverAddress;
            NetAddressInputViewModel = new NetAddressInputViewModel(addressForTesting);

            ClientWcfService.MessageReceived += ClientWcfService_MessageReceived;
        }

        private void ClientWcfService_MessageReceived(object e)
        {
            var dto = e as RtuConnectionCheckedDto;
            if (dto != null)
            {
                var caption = Resources.SID_Connection_check;
                string message = Resources.SID_Can_t_connect_RTU + Environment.NewLine;
                
                if (dto.IsConnectionSuccessfull)
                    message = Resources.SID_RTU_connected_successfully_;
                else
                {
                    var ping = dto.IsPingSuccessful ? Resources.SID____Ping_passed__OK : Resources.SID_Ping_does_not_pass_;
                    message += ping;
                }

                MessageBox.Show(message, caption);
            }
        }

        public void Test()
        {
            if (_serverAddress == null) // check server address
            {
                var doubleAddress = new DoubleAddress() { HasReserveAddress = false, Main = (NetAddress)NetAddressInputViewModel.GetNetAddress().Clone() };
                Result = new C2DWcfManager(doubleAddress, _iniFile, _logFile, _clientId).
                                CheckServerConnection(new CheckServerConnectionDto());

            }
            else // ask server to check rtu address
            {
                var singleServerAddress = new DoubleAddress() { HasReserveAddress = false, Main = (NetAddress)_serverAddress.Clone() };
                new C2DWcfManager(singleServerAddress, _iniFile, _logFile, _clientId)
                    .CheckRtuConnection(new CheckRtuConnectionDto() { NetAddress = (NetAddress)NetAddressInputViewModel.GetNetAddress().Clone() });
            }
        }
    }
}
