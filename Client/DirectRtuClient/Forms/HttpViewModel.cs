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
        public Tests TestsHeader { get; set; }
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

            var d2RL1 = new D2RtuVeexLayer1(_httpExt);
            var d2R = new D2RtuVeexLayer2(_logFile, d2RL1);
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

            var d2RL1 = new D2RtuVeexLayer1(_httpExt);
            var result = await Task.Factory.StartNew(() =>
                d2RL1.SetMonitoringMode(_rtuVeexDoubleAddress, flag ? @"disabled" : @"enabled").Result);

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

            var d2RL1 = new D2RtuVeexLayer1(_httpExt);
            var result = await Task.Factory.StartNew(() =>
                d2RL1.GetTests(_rtuVeexDoubleAddress).Result);

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
                    var test = await Task.Factory.StartNew(() => d2RL1.GetTest(_rtuVeexDoubleAddress, $@"monitoring/{testsItem.self}").Result);
                    _rtuVeexModel.Tests.Add(test);
                    var thresholdSet = await Task.Factory.StartNew(() =>
                        d2RL1.GetTestThresholds(_rtuVeexDoubleAddress, $@"monitoring/{testsItem.self}/thresholds/current").Result);
                    _rtuVeexModel.Thresholds.Add(test.id, thresholdSet);

                    var res1 = await Task.Factory.StartNew(() =>
                        d2RL1.ChangeTest(_rtuVeexDoubleAddress, $@"monitoring/{testsItem.self}", new Test() { state = @"disabled" }).Result);
                    if (res1)
                    {
                        var changedTest = await Task.Factory.StartNew(() =>
                            d2RL1.GetTest(_rtuVeexDoubleAddress, $@"monitoring/{testsItem.self}").Result);
                        Console.WriteLine(changedTest);
                    }
                }

                var firstTest = _rtuVeexModel.TestsHeader.items.First();
                var thresholdSet1 = new ThresholdSet()
                {
                    levels = new List<Level>()
                    {
                        new Level()
                        {
                            groups = new List<Group>()
                            {
                                new Group()
                                {
                                    thresholds = new Thresholds()
                                    {
                                        eventLeadingLossCoefficient = new CombinedThreshold(){decrease = 1},
                                        eventLoss = new CombinedThreshold(){decrease = 2, increase = 2},
                                        eventReflectance = new CombinedThreshold(){min = 4, max = 4}
                                    }
                                }
                            }, name = @"minor"
                        },
                        new Level()
                        {
                            groups = new List<Group>(), name = @"major"
                        },
                        new Level()
                        {
                            groups = new List<Group>(), name = @"critical"
                        },
                    }
                };
                var res = await Task.Factory.StartNew(() =>
                    d2RL1.SetThresholds(_rtuVeexDoubleAddress, $@"monitoring/{firstTest.self}/thresholds", thresholdSet1).Result);
                Console.WriteLine(res);


                var rr = await d2RL1.CreateTest(_rtuVeexDoubleAddress, new CreateTestCmd()
                {
                    id = Guid.NewGuid().ToString(),
                    name = @"precise",
                    otdrId = Guid.Empty.ToString(),
                    otauPort = new OtauPort()
                    {
                        otauId = Guid.Empty.ToString(),
                        portIndex = 1
                    },
                    period = 0,
                    state = @"disable",
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

            var d2RL1 = new D2RtuVeexLayer1(_httpExt);
            var allTests = await Task.Factory.StartNew(() =>
                d2RL1.GetTests(_rtuVeexDoubleAddress).Result);

            if (allTests == null)
            {
                MessageBox.Show(@"Error");
            }
            else
            {
                _rtuVeexModel.TestsHeader = allTests;
                var testItem = _rtuVeexModel.TestsHeader.items.First();
                // var test = await Task.Factory.StartNew(() => d2R.GetTest(_rtuVeexDoubleAddress, $@"monitoring/{testItem.self}").Result);
                var thresholdSet = await Task.Factory.StartNew(() =>
                    d2RL1.GetTestThresholds(_rtuVeexDoubleAddress, $@"monitoring/{testItem.self}/thresholds/current").Result);
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
            var oneBaseRef = new BaseRefDto()
            {
                BaseRefType = BaseRefType.Precise,
                SorBytes = sorBytes,
            };
            var dto = new ReSendBaseRefsDto()
            {
                OtauPortDto = new OtauPortDto()
                {
                    OpticalPort = 1,
                },
                BaseRefDtos = new List<BaseRefDto>()
                {
                    oneBaseRef
                }
            };
            var d2RL1 = new D2RtuVeexLayer1(_httpExt);
            var layer2 = new D2RtuVeexLayer2(_logFile, d2RL1);
            var layer21 = new D2RtuVeexLayer21(d2RL1, layer2);

            var unused = await layer21.FullTestCreation(_rtuVeexDoubleAddress, "", 1, oneBaseRef);

            var layer3 = new D2RtuVeexLayer3(d2RL1, layer2, layer21);
            var result = await Task.Factory.StartNew(() =>
                layer3.ReSendBaseRefsAsync(dto, _rtuVeexDoubleAddress).Result);


            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
            {
                Console.WriteLine(result.ErrorMessage);
            }

            IsButtonEnabled = true;
            ResultString = @"Done";
        }

    }
}
