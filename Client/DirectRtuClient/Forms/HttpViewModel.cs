using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public TestsLinks TestsLinks { get; set; }
        public List<Test> Tests { get; set; }
        public Dictionary<string, ThresholdSet> Thresholds { get; set; } // testId - thresholdSet
    }

    public class HttpViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

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
            _logFile = logFile;

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

            var d2Rl1 = new D2RtuVeexLayer1(_httpExt);
            var d2R = new D2RtuVeexLayer2(_logFile, d2Rl1);
            var result = await Task.Factory.StartNew(() =>
                d2R.GetSettings(_rtuVeexDoubleAddress, new InitializeRtuDto() { RtuAddresses = _rtuVeexDoubleAddress }).Result);

            ResultString = result.ReturnCode.ToString();
            IsButtonEnabled = true;
            if (result.ReturnCode == ReturnCode.RtuInitializedSuccessfully)
                _rtuVeexModel.RtuInitializedDto = result;
            else
                MessageBox.Show(result.ErrorMessage);

            ResultString = result.ReturnCode == ReturnCode.RtuInitializedSuccessfully ? @"Done" : @"Error";
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

            var d2Rl1 = new D2RtuVeexLayer1(_httpExt);
            var result = await Task.Factory.StartNew(() =>
                d2Rl1.SetMonitoringMode(_rtuVeexDoubleAddress, flag ? @"disabled" : @"enabled").Result);

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

            var d2RtuVeexLayer1 = new D2RtuVeexLayer1(_httpExt);
            var result = await d2RtuVeexLayer1.GetTests(_rtuVeexDoubleAddress);

            if (result == null)
            {
                MessageBox.Show(@"Error");
            }
            else
            {
                _rtuVeexModel.TestsLinks = result;
                _rtuVeexModel.Tests = new List<Test>();
                _rtuVeexModel.Thresholds = new Dictionary<string, ThresholdSet>();
                foreach (var testItem in _rtuVeexModel.TestsLinks.Items)
                {
                    var test = await d2RtuVeexLayer1.GetTest(_rtuVeexDoubleAddress, testItem.Self);
                    _rtuVeexModel.Tests.Add(test);
                    var thresholdSet = await d2RtuVeexLayer1.GetTestThresholds(_rtuVeexDoubleAddress, testItem.Self);
                    _rtuVeexModel.Thresholds.Add(test.Id, thresholdSet);

                    var res1 = await d2RtuVeexLayer1.ChangeTest(_rtuVeexDoubleAddress, testItem.Self, new Test() { State = @"disabled" });
                    if (res1)
                    {
                        var changedTest = await d2RtuVeexLayer1.GetTest(_rtuVeexDoubleAddress, testItem.Self);
                        Console.WriteLine(changedTest);
                    }
                }

                var firstTest = _rtuVeexModel.TestsLinks.Items.First();
                var thresholdSet1 = new ThresholdSet()
                {
                    Levels = new List<Level>()
                    {
                        new Level()
                        {
                            Groups = new List<Group>()
                            {
                                new Group()
                                {
                                    Thresholds = new Thresholds()
                                    {
                                        EventLeadingLossCoefficient = new CombinedThreshold(){Decrease = 1},
                                        EventLoss = new CombinedThreshold(){Decrease = 2, Increase = 2},
                                        EventReflectance = new CombinedThreshold(){Min = 4, Max = 4}
                                    }
                                }
                            }, Name = @"minor"
                        },
                        new Level()
                        {
                            Groups = new List<Group>(), Name = @"major"
                        },
                        new Level()
                        {
                            Groups = new List<Group>(), Name = @"critical"
                        },
                    }
                };
                var res = await d2RtuVeexLayer1.SetThresholds(_rtuVeexDoubleAddress, firstTest.Self, thresholdSet1);
                Console.WriteLine(res);


                var rr = await d2RtuVeexLayer1.CreateTest(_rtuVeexDoubleAddress, new CreateTestCmd()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = @"precise",
                    OtdrId = Guid.Empty.ToString(),
                    VeexOtauPort = new VeexOtauPort()
                    {
                        OtauId = Guid.Empty.ToString(),
                        PortIndex = 1
                    },
                    Period = 0,
                    State = @"disable",
                });
                Console.WriteLine(rr.ResponseJson);
            }

            IsButtonEnabled = true;
            ResultString = @"Done";
        }

        public async void GetThresholds()
        {
            ResultString = @"Wait, please";
            IsButtonEnabled = false;

            var d2RtuVeexLayer1 = new D2RtuVeexLayer1(_httpExt);
            var testsLinks = await d2RtuVeexLayer1.GetTests(_rtuVeexDoubleAddress);

            if (testsLinks == null)
            {
                MessageBox.Show(@"Error");
            }
            else
            {
                _rtuVeexModel.TestsLinks = testsLinks;
                var testItem = _rtuVeexModel.TestsLinks.Items.First();
                var thresholdSet = await d2RtuVeexLayer1.GetTestThresholds(_rtuVeexDoubleAddress, testItem.Self);
                if (thresholdSet != null)
                {
                    Console.WriteLine(@"thresholdSet received");
                }
            }

            IsButtonEnabled = true;
            ResultString = @"Done";
        }

        public async void SetPreciseBaseForPort1()
        {
            ResultString = @"Wait, please";
            IsButtonEnabled = false;

            // var sorBytes = File.ReadAllBytes(@"c:\temp\sor\2 nodes 1650.sor");
            var sorBytes = File.ReadAllBytes(@"c:\temp\sor\4p Мстиславль-Хлдосы-Мушино-Селец-Подлужье - Fast.sor");

          //  var sorData = SorData.FromBytes(sorBytes);

            var oneBaseRef = new BaseRefDto()
            {
                BaseRefType = BaseRefType.Precise,
                SorBytes = sorBytes,
            };
            var dto = new AssignBaseRefsDto()
            {
                OtauPortDto = new OtauPortDto()
                {
                    OpticalPort = 1,
                }, 
                
                BaseRefs = new List<BaseRefDto>()
                {
                    oneBaseRef
                }
            };
            var d2Rl1 = new D2RtuVeexLayer1(_httpExt);
            var layer2 = new D2RtuVeexLayer2(_logFile, d2Rl1);
            var layer21 = new D2RtuVeexLayer21(layer2);
            var layer3 = new D2RtuVeexLayer3(layer2, layer21);
            var result = await Task.Factory.StartNew(() =>
                layer3.AssignBaseRefAsync(dto, _rtuVeexDoubleAddress).Result);


            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
            {
                Console.WriteLine(result.ErrorMessage);
            }

            IsButtonEnabled = true;
            ResultString = @"Done";
        }

        public async void GetLastPassed()
        {
            ResultString = @"Wait, please";
            IsButtonEnabled = false;

            var d2Rl1 = new D2RtuVeexLayer1(_httpExt);
            var layer2 = new D2RtuVeexLayer2(_logFile, d2Rl1);

            var rrr = await layer2.GetTestLastMeasurement(_rtuVeexDoubleAddress, @"4dc19b64-7431-435b-9248-621d79d84e0b", @"monitoring_test_passed");
            File.WriteAllBytes(@"c:\temp\0.sor", rrr.SorBytes);

            IsButtonEnabled = true;
            ResultString = @"Done";
        }

        public void EmbedBaseRef()
        {
            ResultString = @"Wait, please";
            IsButtonEnabled = false;

            var baseBytes = File.ReadAllBytes(@"c:\temp\base.sor");
            var measBytes = File.ReadAllBytes(@"c:\temp\meas.sor");

            var measSorData = SorData.FromBytes(measBytes);
            measSorData.EmbedBaseRef(baseBytes);

            measSorData.Save(@"c:\temp\meas-n-base.sor");

            IsButtonEnabled = true;
            ResultString = @"Done";
        }

    }
}
