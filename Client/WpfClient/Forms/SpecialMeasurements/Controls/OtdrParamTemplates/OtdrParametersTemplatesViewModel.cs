using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class OtdrParametersTemplatesViewModel : PropertyChangedBase
    {
        private readonly IniFile _iniFile;
        public OtdrParametersTemplateModel Model { get; set; } = new OtdrParametersTemplateModel();
        private Rtu _rtu;

        public Visibility ListBoxVisibility { get; set; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public OtdrParametersTemplatesViewModel(IniFile iniFile)
        {
            _iniFile = iniFile;
        }

        public void Initialize(Rtu rtu, bool isForRtu)
        {
            _rtu = rtu;
            Model.Initialize(rtu);
            ListBoxVisibility = isForRtu ? Visibility.Collapsed : Visibility.Visible;

            var opUnit = _iniFile.Read(IniSection.OtdrParameters, IniKey.OpUnit, 0);
            Model.Units = rtu.AcceptableMeasParams.Units.Keys.ToList();
            Model.SelectedUnit = Model.Units.Count > opUnit ? Model.Units[opUnit] : Model.Units.First();
            var branchOfAcceptableMeasParams = rtu.AcceptableMeasParams.Units[Model.SelectedUnit];
            Model.BackScatteredCoefficient = branchOfAcceptableMeasParams.BackscatteredCoefficient;
            Model.RefractiveIndex = branchOfAcceptableMeasParams.RefractiveIndex;
        }

        public bool IsAutoLmaxSelected()
        {
            return Model.SelectedOtdrParametersTemplate.Id == 0;
        }

        public List<MeasParamByPosition> GetSelectedParameters()
        {
            var branch = _rtu.AcceptableMeasParams.Units[Model.SelectedUnit];

            var result = new List<MeasParamByPosition>
            {
                new MeasParamByPosition {Param = ServiceFunctionFirstParam.Unit, Position = Model.Units.IndexOf(Model.SelectedUnit)},
                new MeasParamByPosition
                    {Param = ServiceFunctionFirstParam.Bc, Position = (int) (Model.BackScatteredCoefficient * 100)},
                new MeasParamByPosition {Param = ServiceFunctionFirstParam.Ri, Position = (int) (Model.RefractiveIndex * 100000)},
                new MeasParamByPosition {Param = ServiceFunctionFirstParam.IsTime, Position = 1},
            };

            if (Model.SelectedOtdrParametersTemplate.Id != 0)
            {
                var leaf = branch.Distances[Model.SelectedOtdrParametersTemplate.Lmax];
                result.Add(new MeasParamByPosition
                {
                    Param = ServiceFunctionFirstParam.Lmax,
                    Position = branch.Distances.Keys.ToList().IndexOf(Model.SelectedOtdrParametersTemplate.Lmax)
                });
                result.Add(new MeasParamByPosition
                {
                    Param = ServiceFunctionFirstParam.Res,
                    Position = leaf.Resolutions.ToList().IndexOf(Model.SelectedOtdrParametersTemplate.Dl)
                });
                result.Add(new MeasParamByPosition
                {
                    Param = ServiceFunctionFirstParam.Pulse,
                    Position = leaf.PulseDurations.ToList().IndexOf(Model.SelectedOtdrParametersTemplate.Tp)
                });
                result.Add(new MeasParamByPosition
                {
                    Param = ServiceFunctionFirstParam.Time,
                    Position = leaf.PeriodsToAverage.ToList().IndexOf(Model.SelectedOtdrParametersTemplate.Time)
                });
            }

            return result;
        }

        public VeexMeasOtdrParameters GetVeexSelectedParameters()
        {
            var result = new VeexMeasOtdrParameters()
            {
                measurementType = @"manual",
                fastMeasurement = false,
                highFrequencyResolution = false,
                lasers = new List<Laser>() { new Laser() { laserUnit = Model.SelectedUnit } },
                opticalLineProperties = new OpticalLineProperties()
                {
                    kind = @"point_to_point",
                    lasersProperties = new List<LasersProperty>()
                    {
                        new LasersProperty()
                        {
                            laserUnit = Model.SelectedUnit,
                            backscatterCoefficient = (int)Model.BackScatteredCoefficient,
                            refractiveIndex = Model.RefractiveIndex,
                        }
                    }
                },

            };

            if (Model.SelectedOtdrParametersTemplate.Id != 0)
            {
                result.distanceRange = Model.SelectedOtdrParametersTemplate.Lmax;
                result.resolution = Model.SelectedOtdrParametersTemplate.Dl;
                result.pulseDuration = Model.SelectedOtdrParametersTemplate.Tp;
                result.averagingTime = Model.SelectedOtdrParametersTemplate.Time;
            }

            return result;
        }

    }
}
