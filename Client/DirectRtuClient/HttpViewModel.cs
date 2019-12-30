using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace DirectRtuClient
{
    public class RtuVeexModel
    {
        public RtuInitializedDto RtuInitializedDto { get; set; }
        public Tests TestsHeader { get; set; }
        public List<Test> Tests { get; set; }
        public Dictionary<string, ThresholdSet> Thresholds { get; set; } // testId - thresholdSet
    }

    public class HttpViewModel : Screen
    {
        private readonly IniFile _iniFile;

        private readonly DoubleAddress _rtuVeexDoubleAddress;
        private string _resultString;
        private string _rtuVeexAddress;
        private readonly HttpExt _httpExt;

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

        private RtuVeexModel _rtuVeexModel = new RtuVeexModel();
        public HttpViewModel(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;

            _rtuVeexDoubleAddress = iniFile.ReadDoubleAddress(80);
            _httpExt = new HttpExt(logFile);
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

            var d2R = new D2RtuVeex(_httpExt);
            var result = await Task.Factory.StartNew(() =>
                d2R.GetSettings(new InitializeRtuDto() { RtuAddresses = _rtuVeexDoubleAddress }).Result);

            ResultString = result.ReturnCode.ToString();
            IsButtonEnabled = true;
            if (result.ReturnCode == ReturnCode.RtuInitializedSuccessfully)
                _rtuVeexModel.RtuInitializedDto = result;
            else
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

            var d2R = new D2RtuVeexMonitoring(_httpExt);
            var result = await Task.Factory.StartNew(() =>
                d2R.SetMonitoringMode(_rtuVeexDoubleAddress, flag ? @"disabled" : @"enabled").Result);

            ResultString = $@"Stop monitoring result is {result.HttpStatusCode == HttpStatusCode.OK}";
            PatchMonitoringButton = flag ? @"Start monitoring" : @"Stop monitoring";
            IsButtonEnabled = true;
            if (result.HttpStatusCode != HttpStatusCode.OK)
                MessageBox.Show(result.ErrorMessage);
        }

        public async void GetTests()
        {
            ResultString = @"Wait, please";
            IsButtonEnabled = false;

            var d2R = new D2RtuVeexMonitoring(_httpExt);
            var result = await Task.Factory.StartNew(() =>
                d2R.GetTests(_rtuVeexDoubleAddress).Result);



            if (result == null)
            {
                MessageBox.Show(@"Error");
            }
            else
            {
                _rtuVeexModel.TestsHeader = result;
                _rtuVeexModel.Tests = new List<Test>();
                _rtuVeexModel.Thresholds = new Dictionary<string, ThresholdSet>();
                foreach (var testsItem in _rtuVeexModel.TestsHeader.items)
                {
                    var test = await Task.Factory.StartNew(() => d2R.GetTest(_rtuVeexDoubleAddress, $@"monitoring/{testsItem.self}").Result);
                    _rtuVeexModel.Tests.Add(test);
                    var thresholdSet = await Task.Factory.StartNew(() =>
                        d2R.GetTestThresholds(_rtuVeexDoubleAddress, $@"monitoring/tests/{test.thresholds.self}").Result);
                    _rtuVeexModel.Thresholds.Add(test.id, thresholdSet);
                    if (await Task.Factory.StartNew(() =>
                        d2R.ChangeTest(_rtuVeexDoubleAddress, $@"monitoring/{testsItem.self}").Result))
                    {
                        var changedTest = await Task.Factory.StartNew(() => 
                            d2R.GetTest(_rtuVeexDoubleAddress, $@"monitoring/{testsItem.self}").Result);
                    }

                }
            }

            IsButtonEnabled = true;
            ResultString = @"Done";
        }
    }
}
