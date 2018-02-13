using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public static class TreeOfAcceptableMeasParamsExt
    {
        public static void Log(this TreeOfAcceptableMeasParams parameters, IMyLog logFile)
        {
            logFile.EmptyLine();
            foreach (var pair in parameters.Units)
            {
                logFile.AppendLine($@"Wave length {pair.Key}");
                Log(pair.Value, logFile);
            }
        }

        private static void Log(BranchOfAcceptableMeasParams branch, IMyLog logFile)
        {
            logFile.AppendLine($@"RI = {branch.RefractiveIndex}");
            logFile.AppendLine($@"BC = {branch.BackscatteredCoefficient}");

            foreach (var pair in branch.Distances)
            {
                logFile.AppendLine($@"Distance = {pair.Key}");
                Log(pair.Value, logFile);
            }
        }

        private static void Log(LeafOfAcceptableMeasParams leaf, IMyLog logFile)
        {
            logFile.AppendLine($@"resolutions:  {String.Join(@" ;   ", leaf.Resolutions)}"); 
            logFile.AppendLine($@"pulse durations:  {String.Join(@" ;   ", leaf.PulseDurations)}"); 
            logFile.AppendLine($@"time for meas:  {String.Join(@" ;   ", leaf.PeriodsToAverage)}"); 
            logFile.AppendLine($@"count of meas:  {String.Join(@" ;   ", leaf.MeasCountsToAverage)}"); 
        }
    }
    public class OtdrParametersThroughServerSetterViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private TreeOfAcceptableMeasParams _treeOfAcceptableMeasParams;
        public OtdrParametersModel Model { get; set; }
        public bool IsAnswerPositive { get; set; }


        public OtdrParametersThroughServerSetterViewModel(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public void Initialize(TreeOfAcceptableMeasParams treeOfAcceptableMeasParams)
        {
            _treeOfAcceptableMeasParams = treeOfAcceptableMeasParams;
            _treeOfAcceptableMeasParams.Log(_logFile);
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
                MeasParams = new List<Tuple<int, int>>
                {
                    new Tuple<int, int>((int) ServiceFunctionFirstParam.Unit, Model.Units.IndexOf(Model.SelectedUnit)),
                    new Tuple<int, int>((int) ServiceFunctionFirstParam.Bc, (int) (Model.BackscatteredCoefficient * 100)),
                    new Tuple<int, int>((int) ServiceFunctionFirstParam.Ri, (int) (Model.RefractiveIndex * 100000)),
                    new Tuple<int, int>((int) ServiceFunctionFirstParam.Lmax, Model.Distances.IndexOf(Model.SelectedDistance)),
                    new Tuple<int, int>((int) ServiceFunctionFirstParam.Res, Model.Resolutions.IndexOf(Model.SelectedResolution)),
                    new Tuple<int, int>((int) ServiceFunctionFirstParam.Pulse, Model.PulseDurations.IndexOf(Model.SelectedPulseDuration)),
                    new Tuple<int, int>((int) ServiceFunctionFirstParam.IsTime, 1),
                    new Tuple<int, int>((int) ServiceFunctionFirstParam.Time, Model.MeasurementTime.IndexOf(Model.SelectedMeasurementTime)),
                }
            };
            return result;
        }
    }
}
