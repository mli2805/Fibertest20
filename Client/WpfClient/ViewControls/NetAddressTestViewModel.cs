using System;
using System.ComponentModel;
using Caliburn.Micro;
using Dto;
using Iit.Fibertest.UtilsLib;
using WcfConnections;

namespace Iit.Fibertest.Client
{
    public class NetAddressTestViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly LogFile _logFile;
        private readonly Guid _clientId;
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

        public NetAddressTestViewModel(NetAddress netAddress, IniFile iniFile, LogFile logFile, Guid clientId)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _clientId = clientId;
            NetAddressInputViewModel = new NetAddressInputViewModel(netAddress);
        }

        public void Test()
        {
            var doubleAddress = new DoubleAddress() {HasReserveAddress = false, Main = (NetAddress)NetAddressInputViewModel.GetNetAddress().Clone()};
            Result = new C2DWcfManager(doubleAddress, _iniFile, _logFile, _clientId).
                                CheckServerConnection(new CheckServerConnectionDto());
        }
    }
}
