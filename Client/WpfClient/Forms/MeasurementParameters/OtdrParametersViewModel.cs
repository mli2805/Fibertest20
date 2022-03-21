using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class OtdrParametersViewModel : PropertyChangedBase
    {
        private TreeOfAcceptableMeasParams _treeOfAcceptableMeasParams;
        public OtdrParametersModel Model { get; set; }

        public void Initialize(TreeOfAcceptableMeasParams treeOfAcceptableMeasParams)
        {
            _treeOfAcceptableMeasParams = treeOfAcceptableMeasParams;
            Model = new OtdrParametersModel();
            InitializeControls();
            Model.PropertyChanged += Model_PropertyChanged;
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedUnit":
                    ReInitializeForSelectedUnit();
                    break;
                case "SelectedDistance":
                    ReInitializeForSelectedDistance();
                    break;
            }
        }

        private void InitializeControls()
        {
            Model.Units = _treeOfAcceptableMeasParams.Units.Keys.ToList();
            Model.SelectedUnit = Model.Units.First();

            var branchOfAcceptableMeasParams = _treeOfAcceptableMeasParams.Units[Model.SelectedUnit];
            Model.BackscatteredCoefficient = branchOfAcceptableMeasParams.BackscatteredCoefficient;
            Model.RefractiveIndex = branchOfAcceptableMeasParams.RefractiveIndex;
            Model.Distances = branchOfAcceptableMeasParams.Distances
                .Keys.OrderBy(x => double.Parse(x, new CultureInfo(@"en-US"))).ToList();
            Model.SelectedDistance = Model.Distances.First();

            var leafOfAcceptableMeasParams = branchOfAcceptableMeasParams.Distances[Model.SelectedDistance];
            Model.Resolutions = leafOfAcceptableMeasParams.Resolutions.ToList();
            Model.SelectedResolution = Model.Resolutions.First();
            Model.PulseDurations = leafOfAcceptableMeasParams.PulseDurations.ToList();
            Model.SelectedPulseDuration = Model.PulseDurations.First();
            Model.MeasurementTime = leafOfAcceptableMeasParams.PeriodsToAverage.ToList();
            Model.SelectedMeasurementTime = Model.MeasurementTime.First();
        }

        private void ReInitializeForSelectedUnit()
        {
            var branchOfAcceptableMeasParams = _treeOfAcceptableMeasParams.Units[Model.SelectedUnit];
            Model.BackscatteredCoefficient = branchOfAcceptableMeasParams.BackscatteredCoefficient;
            Model.RefractiveIndex = branchOfAcceptableMeasParams.RefractiveIndex;

            var currentDistance = Model.SelectedDistance;
            Model.Distances = branchOfAcceptableMeasParams.Distances.Keys.ToList();
            var index = Model.Distances.IndexOf(currentDistance);
            Model.SelectedDistance = index != -1 ? Model.Distances[index] : Model.Distances.First();
        }

        private void ReInitializeForSelectedDistance()
        {
            var leafOfAcceptableMeasParams =
                _treeOfAcceptableMeasParams.Units[Model.SelectedUnit].Distances[Model.SelectedDistance];

            var currentResolution = Model.SelectedResolution;
            Model.Resolutions = leafOfAcceptableMeasParams.Resolutions.ToList();
            var index = Model.Resolutions.IndexOf(currentResolution);
            Model.SelectedResolution = index != -1 ? Model.Resolutions[index] : Model.Resolutions.First();

            var currentPulseDuration = Model.SelectedPulseDuration;
            Model.PulseDurations = leafOfAcceptableMeasParams.PulseDurations.ToList();
            index = Model.PulseDurations.IndexOf(currentPulseDuration);
            Model.SelectedPulseDuration = index != -1 ? Model.PulseDurations[index] : Model.PulseDurations.First();

            var currentPeriodToAverage = Model.SelectedMeasurementTime;
            Model.MeasurementTime = leafOfAcceptableMeasParams.PeriodsToAverage.ToList();
            index = Model.MeasurementTime.IndexOf(currentPeriodToAverage);
            Model.SelectedMeasurementTime = index != -1 ? Model.MeasurementTime[index] : Model.MeasurementTime.First();
        }

        public List<MeasParam> GetSelectedParameters()
        {
            var result = new List<MeasParam>
            {
                new MeasParam {Param = ServiceFunctionFirstParam.Unit, Value = Model.Units.IndexOf(Model.SelectedUnit)},
                new MeasParam
                    {Param = ServiceFunctionFirstParam.Bc, Value = (int) (Model.BackscatteredCoefficient * 100)},
                new MeasParam {Param = ServiceFunctionFirstParam.Ri, Value = (int) (Model.RefractiveIndex * 100000)},
                new MeasParam
                    {Param = ServiceFunctionFirstParam.Lmax, Value = Model.Distances.IndexOf(Model.SelectedDistance)},
                new MeasParam
                {
                    Param = ServiceFunctionFirstParam.Res, Value = Model.Resolutions.IndexOf(Model.SelectedResolution)
                },
                new MeasParam
                {
                    Param = ServiceFunctionFirstParam.Pulse,
                    Value = Model.PulseDurations.IndexOf(Model.SelectedPulseDuration)
                },
                new MeasParam {Param = ServiceFunctionFirstParam.IsTime, Value = 1},
                new MeasParam
                {
                    Param = ServiceFunctionFirstParam.Time,
                    Value = Model.MeasurementTime.IndexOf(Model.SelectedMeasurementTime)
                },
            };
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
                            backscatterCoefficient = (int)Model.BackscatteredCoefficient,
                            refractiveIndex = Model.RefractiveIndex,
                        }
                    }
                },
                distanceRange = Model.SelectedDistance,
                resolution = Model.SelectedResolution,
                pulseDuration = Model.SelectedPulseDuration,
                averagingTime = Model.SelectedMeasurementTime,
            };

            return result;
        }

    }
}
