using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class NetAddressTestViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly CurrentUser _currentUser;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly NetAddressForConnectionTest _netAddressForConnectionTest;
        private bool? _result;
        private NetAddressInputViewModel _netAddressInputViewModel;

        public NetAddressInputViewModel NetAddressInputViewModel
        {
            get => _netAddressInputViewModel;
            set
            {
                if (Equals(value, _netAddressInputViewModel)) return;
                _netAddressInputViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsButtonEnabled { get; set; }

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

        public NetAddressTestViewModel(ILifetimeScope globalScope, CurrentUser currentUser, IWindowManager windowManager,
            IWcfServiceForClient c2DWcfManager, NetAddressForConnectionTest netAddressForConnectionTest)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _netAddressForConnectionTest = netAddressForConnectionTest;
            NetAddressInputViewModel = new NetAddressInputViewModel(netAddressForConnectionTest.Address, currentUser.Role <= Role.Root);
            IsButtonEnabled = currentUser.Role <= Role.Operator;
            Result = true;
        }

        public async void Test() // button
        {
            if (!NetAddressInputViewModel.IsValidIpAddress())
            {
                _windowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Invalid_IP_address));
                return;
            }

            Result = null;
            bool res;
            using (_globalScope.Resolve<IWaitCursor>())
            {
                res = await TestConnection();
            }

            Result = res;
        }

        public async Task<bool> ExternalTest() // from RTU initialization procedure
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                return await TestConnection();
            }
        }

        public bool IsValidIpAddress()
        {
            return NetAddressInputViewModel.IsValidIpAddress();
        }

        private async Task<bool> TestConnection()
        {
//            if (!NetAddressInputViewModel.IsValidIpAddress())
//            {
//                _windowManager.ShowDialogWithAssignedOwner(
//                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Invalid_IP_address));
//                return false;
//            }
            if (_netAddressForConnectionTest.IsRtuAddress)
            {
                var dto = new CheckRtuConnectionDto()
                {
                    NetAddress = (NetAddress)NetAddressInputViewModel.GetNetAddress().Clone()
                };
                var resultDto = await _c2DWcfManager.CheckRtuConnectionAsync(dto);
                if (resultDto.IsConnectionSuccessfull && dto.NetAddress.Port == -1)
                {
                    NetAddressInputViewModel = new NetAddressInputViewModel(resultDto.NetAddress, _currentUser.Role <= Role.Root);
                }
                return resultDto.IsConnectionSuccessfull;
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
