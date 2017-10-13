using System;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ClientWcfServiceLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class NetAddressTestViewModel : Screen
    {
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly NetAddressForConnectionTest _netAddressForConnectionTest;
        private bool? _result;
        public NetAddressInputViewModel NetAddressInputViewModel { get; set; }


        public bool? Result
        {
            get { return _result; }
            set
            {
                if (value == _result) return;
                _result = value;
                NotifyOfPropertyChange();
            }
        }

        public NetAddressTestViewModel(IWcfServiceForClient c2DWcfManager, NetAddressForConnectionTest netAddressForConnectionTest)
        {
            _c2DWcfManager = c2DWcfManager;
            _netAddressForConnectionTest = netAddressForConnectionTest;
            NetAddressInputViewModel = new NetAddressInputViewModel(netAddressForConnectionTest.Address);
            Result = true;
        }

        public async void Test()
        {
            using (new WaitCursor())
            {
                Result = null;
                Result = await TestConnection();
            }
        }

        private async Task<bool> TestConnection()
        {
            if (_netAddressForConnectionTest.IsRtuAddress)
            {
                var dto = new CheckRtuConnectionDto() {
                    NetAddress = (NetAddress) NetAddressInputViewModel.GetNetAddress().Clone() };
                var b = await _c2DWcfManager.CheckRtuConnectionAsync(dto);
                return b.IsConnectionSuccessfull;
            }
            else // DataCenter testing
            {
                var addressForTesting = new DoubleAddress()
                {
                    HasReserveAddress = false,
                    Main = (NetAddress) NetAddressInputViewModel.GetNetAddress().Clone()
                };
                ((C2DWcfManager) _c2DWcfManager).SetServerAddresses(addressForTesting);
                return await _c2DWcfManager.CheckServerConnection(new CheckServerConnectionDto());
            }
        }
    }
}
