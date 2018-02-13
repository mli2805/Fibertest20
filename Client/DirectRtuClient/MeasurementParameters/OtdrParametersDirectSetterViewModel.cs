using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;

namespace DirectRtuClient
{
    public class OtdrParametersDirectSetterViewModel : Screen
    {
        #region Combo-boxes
        private string _selectedUnit;
        private double _backscatteredCoefficient;
        private double _refractiveIndex;
        private List<string> _distances;
        private string _selectedDistance;
        private List<string> _resolutions;
        private string _selectedResolution;
        private List<string> _pulseDurations;
        private string _selectedPulseDuration;
        private List<string> _measCountsToAverage;
        private string _selectedMeasCountToAverage;
        private List<string> _periodsToAverage;
        private string _selectedPeriodToAverage;
        private bool _isTimeToAverageSelected;

        public List<string> Units { get; set; }

        public string SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (value == _selectedUnit) return;
                _selectedUnit = value;
                NotifyOfPropertyChange();

                _interOpWrapper.SetParam(ServiceFunctionFirstParam.Unit, Units.IndexOf(SelectedUnit));
                InitializeForSelectedUnit();
            }
        }

        public double BackscatteredCoefficient
        {
            get => _backscatteredCoefficient;
            set
            {
                if (value.Equals(_backscatteredCoefficient)) return;
                _backscatteredCoefficient = value;
                NotifyOfPropertyChange();

                _interOpWrapper.SetParam(ServiceFunctionFirstParam.Bc, (int)(BackscatteredCoefficient*100));
                InitializeFromSelectedDistance();
            }
        }

        public double RefractiveIndex
        {
            get => _refractiveIndex;
            set
            {
                if (value.Equals(_refractiveIndex)) return;
                _refractiveIndex = value;
                NotifyOfPropertyChange();

                _interOpWrapper.SetParam(ServiceFunctionFirstParam.Ri, (int)(RefractiveIndex*100000));
                InitializeFromSelectedDistance();
            }
        }

        public List<string> Distances
        {
            get => _distances;
            set
            {
                if (Equals(value, _distances)) return;
                _distances = value;
                NotifyOfPropertyChange();
            }
        }

        public string SelectedDistance
        {
            get => _selectedDistance;
            set
            {
                if (value == _selectedDistance) return;
                _selectedDistance = value;
                NotifyOfPropertyChange();

                _interOpWrapper.SetParam(ServiceFunctionFirstParam.Lmax, Distances.IndexOf(SelectedDistance));
                InitializeFromSelectedDistance();
            }
        }

        public List<string> Resolutions
        {
            get => _resolutions;
            set
            {
                if (Equals(value, _resolutions)) return;
                _resolutions = value;
                NotifyOfPropertyChange();
            }
        }

        public string SelectedResolution
        {
            get => _selectedResolution;
            set
            {
                if (value == _selectedResolution) return;
                _selectedResolution = value;
                NotifyOfPropertyChange();

                var indexInLine = Resolutions.IndexOf(SelectedResolution) + 1; // AUTO was excluded
                _interOpWrapper.SetParam(ServiceFunctionFirstParam.Res, indexInLine);
                InitializeFromSelectedResolution();
            }
        }

        public List<string> PulseDurations
        {
            get => _pulseDurations;
            set
            {
                if (Equals(value, _pulseDurations)) return;
                _pulseDurations = value;
                NotifyOfPropertyChange();
            }
        }

        public string SelectedPulseDuration
        {
            get => _selectedPulseDuration;
            set
            {
                if (value == _selectedPulseDuration) return;
                _selectedPulseDuration = value;
                NotifyOfPropertyChange();
                _interOpWrapper.SetParam(ServiceFunctionFirstParam.Pulse, PulseDurations.IndexOf(SelectedPulseDuration));
            }
        }

        public List<string> MeasCountsToAverage
        {
            get => _measCountsToAverage;
            set
            {
                if (Equals(value, _measCountsToAverage)) return;
                _measCountsToAverage = value;
                NotifyOfPropertyChange();
            }
        }

        public string SelectedMeasCountToAverage
        {
            get => _selectedMeasCountToAverage;
            set
            {
                if (value == _selectedMeasCountToAverage) return;
                _selectedMeasCountToAverage = value;
                NotifyOfPropertyChange();

                if (!IsTimeToAverageSelected)
                {
                    _interOpWrapper.SetParam(ServiceFunctionFirstParam.Navr,
                        MeasCountsToAverage.IndexOf(SelectedMeasCountToAverage));
                    PeriodsToAverage = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Time).ToList();
                    SelectedPeriodToAverage = PeriodsToAverage.First();
                }
            }
        }

        public List<string> PeriodsToAverage
        {
            get => _periodsToAverage;
            set
            {
                if (Equals(value, _periodsToAverage)) return;
                _periodsToAverage = value;
                NotifyOfPropertyChange();
            }
        }

        public string SelectedPeriodToAverage
        {
            get => _selectedPeriodToAverage;
            set
            {
                if (value == _selectedPeriodToAverage) return;
                _selectedPeriodToAverage = value;
                NotifyOfPropertyChange();

                if (IsTimeToAverageSelected)
                {
                    _interOpWrapper.SetParam(ServiceFunctionFirstParam.Time, PeriodsToAverage.IndexOf(SelectedPeriodToAverage));
                    MeasCountsToAverage = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Navr).ToList();
                    SelectedMeasCountToAverage = MeasCountsToAverage.First();
                }
            }
        }

        public bool IsTimeToAverageSelected
        {
            get => _isTimeToAverageSelected;
            set
            {
                if (value == _isTimeToAverageSelected) return;
                _isTimeToAverageSelected = value;
                NotifyOfPropertyChange();

                _interOpWrapper.SetParam(ServiceFunctionFirstParam.IsTime, IsTimeToAverageSelected ? 1 : 0);
                if (IsTimeToAverageSelected)
                {
                    PeriodsToAverage = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Time).ToList();
                    SelectedPeriodToAverage = PeriodsToAverage.First();
                }
                else
                {
                    MeasCountsToAverage = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Navr).ToList();
                    SelectedMeasCountToAverage = MeasCountsToAverage[2];
                }
            }
        }
        public bool IsMeasCountToAverageSelected { get; set; }
        #endregion

        private readonly InterOpWrapper _interOpWrapper;

        public OtdrParametersDirectSetterViewModel(InterOpWrapper interOpWrapper)
        {
            _interOpWrapper = interOpWrapper;

            InitializeControls();
        }

        private void InitializeControls()
        {
            Units = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Unit).ToList();
            _selectedUnit = Units.First();

            _backscatteredCoefficient = double.Parse(_interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Bc)[0], new CultureInfo("en-US"));
            _refractiveIndex = double.Parse(_interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Ri)[0], new CultureInfo("en-US"));

            Distances = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Lmax).ToList();
            var activeDistance = _interOpWrapper.GetLineOfVariantsForParam(ServiceFunctionFirstParam.ActiveLmax);
            var index = Distances.IndexOf(activeDistance);
            _selectedDistance = index != -1 ? Distances[index] : Distances.First();

            Resolutions = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Res).Skip(1).ToList();
            var activeResolution = _interOpWrapper.GetLineOfVariantsForParam(ServiceFunctionFirstParam.ActiveRes);
            index = Resolutions.IndexOf(activeResolution);
            _selectedResolution = index != -1 ? Resolutions[index] : Resolutions[1];

            PulseDurations = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Pulse).ToList();
            var activePulseDuration = _interOpWrapper.GetLineOfVariantsForParam(ServiceFunctionFirstParam.ActivePulse);
            index = PulseDurations.IndexOf(activePulseDuration);
            _selectedPulseDuration = index != -1 ? PulseDurations[index] : PulseDurations.First();

            _isTimeToAverageSelected = int.Parse(_interOpWrapper.GetLineOfVariantsForParam(ServiceFunctionFirstParam.ActiveIsTime)) == 1;
            if (_isTimeToAverageSelected)
            {
                PeriodsToAverage = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Time).ToList();
                var activePeriodToAverage = _interOpWrapper.GetLineOfVariantsForParam(ServiceFunctionFirstParam.ActiveTime);
                index = PeriodsToAverage.IndexOf(activePeriodToAverage);
                _selectedPeriodToAverage = index != -1 ? PeriodsToAverage[index] : PeriodsToAverage.First();

                MeasCountsToAverage = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Navr).ToList();
                _selectedMeasCountToAverage = MeasCountsToAverage.First();
            }
            else
            {
                MeasCountsToAverage = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Navr).ToList();
                var activeMeasCountToAverage = _interOpWrapper.GetLineOfVariantsForParam(ServiceFunctionFirstParam.ActiveNavr);
                index = MeasCountsToAverage.IndexOf(activeMeasCountToAverage);
                _selectedMeasCountToAverage = index != -1 ? MeasCountsToAverage[index] : MeasCountsToAverage.First();

                PeriodsToAverage = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Time).ToList();
                _selectedPeriodToAverage = PeriodsToAverage.First();
            }
            IsMeasCountToAverageSelected = !IsTimeToAverageSelected;
        }

        private void InitializeForSelectedUnit()
        {
            _backscatteredCoefficient = double.Parse(_interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Bc)[0], new CultureInfo("en-US"));
            _refractiveIndex = double.Parse(_interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Ri)[0], new CultureInfo("en-US"));
            Distances = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Lmax).ToList();
            var activeDistance = _interOpWrapper.GetLineOfVariantsForParam(ServiceFunctionFirstParam.ActiveLmax);
            var index = Distances.IndexOf(activeDistance);
            SelectedDistance = index != -1 ? Distances[index] : Distances.First();
        }

        private void InitializeFromSelectedDistance()
        {
            Resolutions = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Res).Skip(1).ToList();
            var activeResolution = _interOpWrapper.GetLineOfVariantsForParam(ServiceFunctionFirstParam.ActiveRes);
            var index1 = Resolutions.IndexOf(activeResolution);
            SelectedResolution = index1 != -1 ? Resolutions[index1] : Resolutions[1];

            PulseDurations = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Pulse).ToList();
            var activePulseDuration = _interOpWrapper.GetLineOfVariantsForParam(ServiceFunctionFirstParam.ActivePulse);
            var index = PulseDurations.IndexOf(activePulseDuration);
            SelectedPulseDuration = index != -1 ? PulseDurations[index] : PulseDurations.First();
        }

        private void InitializeFromSelectedResolution()
        {
            IsTimeToAverageSelected =
                int.Parse(_interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.IsTime)[0]) == 1;
            if (IsTimeToAverageSelected)
            {
                PeriodsToAverage = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Time).ToList();
                var activePeriodToAverage = _interOpWrapper.GetLineOfVariantsForParam(ServiceFunctionFirstParam.ActiveTime);
                var index = PeriodsToAverage.IndexOf(activePeriodToAverage);
                SelectedPeriodToAverage = index != -1 ? PeriodsToAverage[index] : PeriodsToAverage.First();

                MeasCountsToAverage = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Navr).ToList();
                SelectedMeasCountToAverage = MeasCountsToAverage.First();
            }
            else
            {
                MeasCountsToAverage = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Navr).ToList();
                var activeMeasCountToAverage = _interOpWrapper.GetLineOfVariantsForParam(ServiceFunctionFirstParam.ActiveNavr);
                var index = MeasCountsToAverage.IndexOf(activeMeasCountToAverage);
                SelectedMeasCountToAverage = index != -1 ? MeasCountsToAverage[index] : MeasCountsToAverage.First();

                PeriodsToAverage = _interOpWrapper.ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Time).ToList();
                SelectedPeriodToAverage = PeriodsToAverage.First();
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Measurement_parameters;
        }

        public void Close()
        {
            TryClose();
        }


    }
}
