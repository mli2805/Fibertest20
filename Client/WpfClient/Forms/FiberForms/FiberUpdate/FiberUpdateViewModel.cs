﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class FiberUpdateViewModel : Screen, IDataErrorInfo
    {
        private readonly Model _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly CurrentGis _currentGis;
        private readonly GraphGpsCalculator _graphGpsCalculator;
        private readonly ReflectogramManager _reflectogramManager;
        private Fiber _fiber;
        private string _userInputedLength;

        public string GpsLength { get; set; }

        public string NodeAtitle { get; set; }
        public string NodeBtitle { get; set; }

        public string UserInputedLength
        {
            get => _userInputedLength;
            set
            {
                if (value.Equals(_userInputedLength)) return;
                _userInputedLength = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsEditEnabled { get; set; }
        public bool IsButtonSaveEnabled { get; set; }
        public UpdateFiber Command { get; set; }

        public Tuple<Trace, string> SelectedTrace { get; set; }
        public List<Tuple<Trace, string>> TracesThrough { get; set; } = new List<Tuple<Trace, string>>();

        public Visibility GisVisibility { get; set; }

        public FiberUpdateViewModel(Model readModel, GraphReadModel graphReadModel,
            CurrentUser currentUser, CurrentlyHiddenRtu currentlyHiddenRtu, CurrentGis currentGis,
            GraphGpsCalculator graphGpsCalculator, ReflectogramManager reflectogramManager)
        {
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _currentGis = currentGis;
            IsEditEnabled = currentUser.Role <= Role.Root;
            _graphGpsCalculator = graphGpsCalculator;
            _reflectogramManager = reflectogramManager;
        }

        public async Task Initialize(Guid fiberId)
        {
            GisVisibility = _currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
            _fiber = _readModel.Fibers.Single(f => f.FiberId == fiberId);
             GpsLength = $@"{_graphGpsCalculator.GetFiberFullGpsDistance(fiberId, out Node node1, out Node node2):#,##0}";
            NodeAtitle = node1.Title;
            NodeBtitle = node2.Title;
         
            foreach (var trace in _readModel.Traces)
            {
                var index = trace.FiberIds.IndexOf(fiberId);
                if (index != -1)
                {
                    var realIndex = GetRealFiberIndex(trace, index);
                    TracesThrough.Add(new Tuple<Trace, string>(trace, await GetOpticalLength(trace, realIndex)));
                    if (trace.IsIncludedInMonitoringCycle)
                    {
                        var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
                        if (rtu == null) continue;
                        if (rtu.MonitoringState == MonitoringState.On)
                            IsEditEnabled = false;
                    }
                }
            }
            SelectedTrace = TracesThrough.FirstOrDefault();

            UserInputedLength = _fiber.UserInputedLength.Equals(0) ? "" : _fiber.UserInputedLength.ToString(CultureInfo.InvariantCulture);
        }

        private int GetRealFiberIndex(Trace trace, int fiberIndexWithAdjustmentPoints)
        {
            var counter = 0;

            for (int i = 1; i <= fiberIndexWithAdjustmentPoints; i++)
            {
                var equipment = _readModel.Equipments.First(e=>e.EquipmentId == trace.EquipmentIds[i]);
                if (equipment.Type != EquipmentType.AdjustmentPoint)
                    counter++;
            }

            return counter;
        }

        private async Task<string> GetOpticalLength(Trace trace, int index)
        {
            if (trace.PreciseId == Guid.Empty)
                return Resources.SID_no_base;
            var sorFileId = _readModel.BaseRefs.First(b => b.Id == trace.PreciseId).SorFileId;
            var sorBytes = await _reflectogramManager.GetSorBytes(sorFileId);
            var otdrKnownBlocks = SorData.FromBytes(sorBytes);
            var result = otdrKnownBlocks.GetDistanceBetweenLandmarksInMm(index, index + 1) / 1000;
            return result.ToString();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Section;
        }

        public void ShowTrace()
        {
            if (SelectedTrace == null) return;

            if (_currentlyHiddenRtu.Collection.Contains(SelectedTrace.Item1.RtuId))
            {
                _currentlyHiddenRtu.Collection.Remove(SelectedTrace.Item1.RtuId);
                _currentlyHiddenRtu.ChangedRtu = SelectedTrace.Item1.RtuId;
            }
            _graphReadModel.HighlightTrace(SelectedTrace.Item1.NodeIds[0], SelectedTrace.Item1.FiberIds);
        }

        public void Save()
        {
            int userInputedLength = 0;
            if (_userInputedLength != "")
                int.TryParse(_userInputedLength, out userInputedLength);
            Command = new UpdateFiber { Id = _fiber.FiberId, UserInputedLength = userInputedLength };
            TryClose();
        }

        public void Cancel()
        {
            Command = null;
            TryClose();
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "UserInputedLength":
                        if (_userInputedLength != "" && !int.TryParse(_userInputedLength, out _))
                        {
                            errorMessage = Resources.SID_Length_should_be_a_number;
                        }
                        IsButtonSaveEnabled = errorMessage == string.Empty;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; } = null;
    }
}
