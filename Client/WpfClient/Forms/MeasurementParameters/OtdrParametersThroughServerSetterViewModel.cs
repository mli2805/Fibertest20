using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
   
    public class OtdrParametersThroughServerSetterViewModel : Screen
    {
        private TreeOfAcceptableMeasParams _treeOfAcceptableMeasParams;
        public OtdrParametersModel Model { get; set; }
        public bool IsAnswerPositive { get; set; }


        public void Initialize(TreeOfAcceptableMeasParams treeOfAcceptableMeasParams)
        {
            _treeOfAcceptableMeasParams = treeOfAcceptableMeasParams;
            File.WriteAllLines(@"..\temp\tree.txt", _treeOfAcceptableMeasParams.Log());
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
            Model.Distances = branchOfAcceptableMeasParams.Distances.Keys.ToList();
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
            Model.SelectedDistance = index != -1 ?  Model.Distances[index] : Model.Distances.First();
        }

        private void ReInitializeForSelectedDistance()
        {
            var leafOfAcceptableMeasParams = _treeOfAcceptableMeasParams.Units[Model.SelectedUnit].Distances[Model.SelectedDistance];

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

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Measurement_parameters;
        }

        public void Measure()
        {
            IsAnswerPositive = true;
            TryClose();
        }

        public void Close()
        {
            IsAnswerPositive = false;
            TryClose();
        }

        public SelectedMeasParams GetSelectedParameters()
        {
            var result = new SelectedMeasParams
            {
                MeasParams = new List<Tuple<ServiceFunctionFirstParam, int>>
                {
                    new Tuple<ServiceFunctionFirstParam, int>(ServiceFunctionFirstParam.Unit, Model.Units.IndexOf(Model.SelectedUnit)),
                    new Tuple<ServiceFunctionFirstParam, int>(ServiceFunctionFirstParam.Bc, (int) (Model.BackscatteredCoefficient * 100)),
                    new Tuple<ServiceFunctionFirstParam, int>(ServiceFunctionFirstParam.Ri, (int) (Model.RefractiveIndex * 100000)),
                    new Tuple<ServiceFunctionFirstParam, int>(ServiceFunctionFirstParam.Lmax, Model.Distances.IndexOf(Model.SelectedDistance)),
                    new Tuple<ServiceFunctionFirstParam, int>(ServiceFunctionFirstParam.Res, Model.Resolutions.IndexOf(Model.SelectedResolution)),
                    new Tuple<ServiceFunctionFirstParam, int>(ServiceFunctionFirstParam.Pulse, Model.PulseDurations.IndexOf(Model.SelectedPulseDuration)),
                    new Tuple<ServiceFunctionFirstParam, int>(ServiceFunctionFirstParam.IsTime, 1),
                    new Tuple<ServiceFunctionFirstParam, int>(ServiceFunctionFirstParam.Time, Model.MeasurementTime.IndexOf(Model.SelectedMeasurementTime)),
                }
            };
            return result;
        }
    }
}
