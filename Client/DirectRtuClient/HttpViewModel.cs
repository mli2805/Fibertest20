using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using D2RtuVeexManager;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace DirectRtuClient
{
    public class HttpViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        private readonly DoubleAddress _rtuVeexDoubleAddress;
        private string _resultString;
        private string _rtuVeexAddress;

        public string RtuVeexAddress
        {
            get => _rtuVeexAddress;
            set
            {
                _rtuVeexAddress = value;
                SaveAddress();
            }
        }

        public string ResultString
        {
            get => _resultString;
            set
            {
                if (value == _resultString) return;
                _resultString = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonEnabled = true;
        private string _patchMonitoringButton = @"Stop monitoring";

        public bool IsButtonEnabled
        {
            get => _isButtonEnabled;
            set
            {
                if (value == _isButtonEnabled) return;
                _isButtonEnabled = value;
                NotifyOfPropertyChange();
            }
        }

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

        public async void GetSettings()
        {
            ResultString = @"Wait, please";
            IsButtonEnabled = false;
          
            var d2R = new D2RtuVeex(_logFile);
            var result = await Task.Factory.StartNew(() =>
                d2R.GetSettings(new InitializeRtuDto() {RtuAddresses = _rtuVeexDoubleAddress}).Result);

            ResultString = result.ReturnCode.ToString();
            IsButtonEnabled = true;
            if (result.ReturnCode != ReturnCode.RtuInitializedSuccessfully)
                MessageBox.Show(result.ErrorMessage);
        }

        public string PatchMonitoringButton
        {
            get => _patchMonitoringButton;
            set
            {
                if (value == _patchMonitoringButton) return;
                _patchMonitoringButton = value;
                NotifyOfPropertyChange();
            }
        }

        public async void PatchMonitoring()
        {
            ResultString = @"Wait, please";
            IsButtonEnabled = false;
            var flag = PatchMonitoringButton == @"Stop monitoring";
          
            var d2R = new D2RtuVeexMonitoring(_logFile);
            var result = await Task.Factory.StartNew(() =>
                d2R.Monitoring(_rtuVeexDoubleAddress, @"monitoring", flag ? @"disabled" : @"enabled").Result);

            ResultString = result.ReturnCode.ToString();
            PatchMonitoringButton = flag ? @"Start monitoring" : @"Stop monitoring";
            IsButtonEnabled = true;
            if (result.ReturnCode != ReturnCode.MonitoringSettingsAppliedSuccessfully)
                MessageBox.Show(result.ErrorMessage);
        }

    }
}
