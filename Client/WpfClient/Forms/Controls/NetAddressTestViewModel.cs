using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class NetAddressTestViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
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

        public NetAddressTestViewModel(ILifetimeScope globalScope, IWcfServiceForClient c2DWcfManager, NetAddressForConnectionTest netAddressForConnectionTest)
        {
            _globalScope = globalScope;
            _c2DWcfManager = c2DWcfManager;
            _netAddressForConnectionTest = netAddressForConnectionTest;
            NetAddressInputViewModel = new NetAddressInputViewModel(netAddressForConnectionTest.Address);
            Result = true;
        }

        public async void Test()
        {
            Result = null;
            var res = false;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                res = await TestConnection();
            }
            Result = res;
        }

        public async Task<bool> ExternalTest()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                return await TestConnection();
            }
        }

        private async Task<bool> TestConnection()
        {
            if (_netAddressForConnectionTest.IsRtuAddress)
            {
                var dto = new CheckRtuConnectionDto()
                {
                    NetAddress = (NetAddress)NetAddressInputViewModel.GetNetAddress().Clone()
                };
                var b = await _c2DWcfManager.CheckRtuConnectionAsync(dto);
                return b.IsConnectionSuccessfull;
            }
            else // DataCenter testing
            {
                var addressForTesting = new DoubleAddress()
                {
                    HasReserveAddress = false,
                    Main = (NetAddress)NetAddressInputViewModel.GetNetAddress().Clone()
                };
                ((C2DWcfManager)_c2DWcfManager).SetServerAddresses(addressForTesting, "", "");
                return await _c2DWcfManager.CheckServerConnection(new CheckServerConnectionDto());
            }
        }
    }
}
