using Caliburn.Micro;
using HttpLib;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace DirectRtuClient
{
    public class HttpViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        private readonly DoubleAddress _rtuVeexDoubleAddress;
        public string RtuVeexAddress { get; set; }

        public HttpViewModel(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;

            _rtuVeexDoubleAddress = iniFile.ReadDoubleAddress(80);
            RtuVeexAddress = _rtuVeexDoubleAddress.Main.Ip4Address;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"Http";
        }

        public void SaveAddress()
        {
            _rtuVeexDoubleAddress.Main.Ip4Address = RtuVeexAddress;
            _iniFile.WriteServerAddresses(_rtuVeexDoubleAddress);
        }

        public async void GetOtdrs()
        {
            var d2RHttpManager = new D2RHttpManager();
            d2RHttpManager.Initialize(_rtuVeexDoubleAddress, _logFile);

            var result = await d2RHttpManager.GetSettings(new InitializeRtuDto());
            _logFile.AppendLine(result.ReturnCode.ToString());
        }

    }
}
