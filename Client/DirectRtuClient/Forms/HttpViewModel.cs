﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            var d2Rl1 = new D2RtuVeexLayer1(_httpExt);
            var d2R = new D2RtuVeexLayer2(d2Rl1);
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

        public async void GetTests()
        {
            ResultString = @"Wait, please";
            IsButtonEnabled = false;

            var d2RtuVeexLayer1 = new D2RtuVeexLayer1(_httpExt);
            var result = await d2RtuVeexLayer1.GetTests(_rtuVeexDoubleAddress);

            if (!result.IsSuccessful)
            {
                MessageBox.Show(@"Error");
            }
            else
            {
                _rtuVeexModel.TestsLinks = (TestsLinks)result.ResponseObject;
                _rtuVeexModel.Tests = new List<Test>();
                _rtuVeexModel.Thresholds = new Dictionary<string, ThresholdSet>();
                foreach (var testItem in _rtuVeexModel.TestsLinks.items)
                {
                    var testRes = await d2RtuVeexLayer1.GetTest(_rtuVeexDoubleAddress, testItem.self);
                    if (!testRes.IsSuccessful) return;
                    var test = (Test)testRes.ResponseObject;
                    _rtuVeexModel.Tests.Add(test);
                    var getResult = await d2RtuVeexLayer1.GetTestThresholds(_rtuVeexDoubleAddress, testItem.self);

                    var thresholdSet = (ThresholdSet)getResult.ResponseObject;
                    _rtuVeexModel.Thresholds.Add(test.id, thresholdSet);

                    var res1 = await d2RtuVeexLayer1.ChangeTest(_rtuVeexDoubleAddress, testItem.self, new Test() { state = @"disabled" });
                    if (res1.IsSuccessful)
                    {
                        var changedTest = await d2RtuVeexLayer1.GetTest(_rtuVeexDoubleAddress, testItem.self);
                        Console.WriteLine(changedTest);
                    }
                }

                var firstTest = _rtuVeexModel.TestsLinks.items.First();
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
                var res = await d2RtuVeexLayer1.SetThresholds(_rtuVeexDoubleAddress, firstTest.self, thresholdSet1);
                Console.WriteLine(res);


                var rr = await d2RtuVeexLayer1.CreateTest(_rtuVeexDoubleAddress, new Test()
                {
                    id = Guid.NewGuid().ToString(),
                    name = @"precise",
                    otdrId = Guid.Empty.ToString(),
                    otauPorts = new List<VeexOtauPort>()
                    {
                        new VeexOtauPort()
                        {
                            otauId = Guid.Empty.ToString(),
                            portIndex = 1
                        }
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

            var d2RtuVeexLayer1 = new D2RtuVeexLayer1(_httpExt);
            var getRes = await d2RtuVeexLayer1.GetTests(_rtuVeexDoubleAddress);

            if (!getRes.IsSuccessful)
            {
                MessageBox.Show(@"Error");
            }
            else
            {
                _rtuVeexModel.TestsLinks = (TestsLinks)getRes.ResponseObject;
                var testItem = _rtuVeexModel.TestsLinks.items.First();
                var thresholdSet = await d2RtuVeexLayer1.GetTestThresholds(_rtuVeexDoubleAddress, testItem.self);
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
            var layer2 = new D2RtuVeexLayer2(d2Rl1);
            var layer3 = new D2RtuVeexLayer3(layer2);
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
            var layer2 = new D2RtuVeexLayer2(d2Rl1);

            var rrr = await layer2.GetTestLastMeasurement(_rtuVeexDoubleAddress, @"4dc19b64-7431-435b-9248-621d79d84e0b", @"monitoring_test_passed", false);
            File.WriteAllBytes(@"c:\temp\0.sor", rrr.SorBytes);

            IsButtonEnabled = true;
            ResultString = @"Done";
        }

        public async void CreateOtau()
        {
            var d2Rl1 = new D2RtuVeexLayer1(_httpExt);
            await d2Rl1.CreateOtau(_rtuVeexDoubleAddress,
                new NewOtau()
                {
                    connectionParameters = new VeexOtauAddress() {address = @"192.168.96.237", port = 4001},
                });
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
