using System;
using System.Windows;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
using Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using WcfConnections;

namespace Iit.Fibertest.Client
{
    public class NetAddressTestViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly LogFile _logFile;
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

        public NetAddressTestViewModel(NetAddress addressForTesting, IniFile iniFile, LogFile logFile, Guid clientId, NetAddress serverAddress = null)
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
                string message = Resources.SID_Cant_establish_connection_ + Environment.NewLine;
                
                if (dto.IsRtuInitialized)
                    message = Resources.SID_Connection_established__RTU_is_Ok_;
                else if (dto.IsServiceStarted)
                    message = Resources.SID_Connection_established_Rtu_is_initializing_now;
                else
                    message += dto.IsPingSuccessful ? Resources.SID_Ping_passed_ : Resources.SID_Ping_failed;

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
