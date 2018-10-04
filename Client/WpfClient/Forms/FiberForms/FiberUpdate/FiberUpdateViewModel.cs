﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class FiberUpdateViewModel : Screen, IDataErrorInfo
    {
        private readonly Model _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
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


        public FiberUpdateViewModel(Model readModel, GraphReadModel graphReadModel,
            CurrentUser currentUser, CurrentlyHiddenRtu currentlyHiddenRtu,
            GraphGpsCalculator graphGpsCalculator, ReflectogramManager reflectogramManager)
        {
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            IsEditEnabled = currentUser.Role <= Role.Root;
            _graphGpsCalculator = graphGpsCalculator;
            _reflectogramManager = reflectogramManager;
        }

        public async Task Initialize(Guid fiberId)
        {
            _fiber = _readModel.Fibers.Single(f => f.FiberId == fiberId);
            NodeAtitle = _readModel.Nodes.Single(n => n.NodeId == _fiber.NodeId1).Title;
            NodeBtitle = _readModel.Nodes.Single(n => n.NodeId == _fiber.NodeId2).Title;

            GpsLength = $@"{_graphGpsCalculator.GetFiberFullGpsDistance(fiberId):#,##0}";

            foreach (var trace in _readModel.Traces)
            {
                var index = trace.FiberIds.IndexOf(fiberId);
                if (index != -1)
                    TracesThrough.Add(new Tuple<Trace, string>(trace, await GetOpticalLength(trace, index)));
            }
            SelectedTrace = TracesThrough.FirstOrDefault();

            UserInputedLength = _fiber.UserInputedLength.Equals(0) ? "" : _fiber.UserInputedLength.ToString(CultureInfo.InvariantCulture);
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
