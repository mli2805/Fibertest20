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
        private bool _isRtuTesting;
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

        public NetAddressTestViewModel(IWcfServiceForClient c2DWcfManager)
        {
            _c2DWcfManager = c2DWcfManager;
            Result = true;
        }

        public void Init(NetAddress addressForTesting, bool isRtuTesting)
        {
            _isRtuTesting = isRtuTesting;
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

        public async void Test()
        {
            Result = null;
            Result = await TestConnection();
        }

        private async Task<bool> TestConnection()
        {
            if (_isRtuTesting)
            {
                var b = await _c2DWcfManager
                    .CheckRtuConnectionAsync(new CheckRtuConnectionDto()
                    {
                        NetAddress = (NetAddress) NetAddressInputViewModel.GetNetAddress().Clone()
                    });
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
