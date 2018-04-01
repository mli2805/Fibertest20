using System;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Algorithms;

namespace Iit.Fibertest.Client
{
    public class AccidentEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly AccidentPlaceLocator _accidentPlaceLocator;

        public AccidentEventsOnGraphExecutor(GraphReadModel model, Model readModel, 
            CurrentUser currentUser, AccidentPlaceLocator accidentPlaceLocator)
        {
            _model = model;
            _readModel = readModel;
            _currentUser = currentUser;
            _accidentPlaceLocator = accidentPlaceLocator;
        }

        public void ShowMonitoringResult(MeasurementAdded evnt)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Traces.First(t => t.TraceId == evnt.TraceId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            _model.ChangeTraceColor(evnt.TraceId, evnt.TraceState);

            _model.CleanAccidentPlacesOnTrace(evnt.TraceId); // accidents on trace could change, so old should be cleaned and new drawn
            if (evnt.TraceState != FiberState.Ok && evnt.TraceState != FiberState.NoFiber)
                evnt.Accidents.ForEach(a => ShowAccidentPlaceOnTrace(a, evnt.TraceId));
        }

        private void ShowAccidentPlaceOnTrace(AccidentOnTrace accident, Guid traceId)
        {
            switch (accident)
            {
                case AccidentAsNewEvent accidentAsNewEvent:
                    ShowBetweenNodes(accidentAsNewEvent, traceId);
                    return;
                case AccidentInOldEvent accidentInOldEvent:
                    if (accidentInOldEvent.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff)
                        ShowBadSegment(accidentInOldEvent, traceId);
                    else
                        ShowInNode(accidentInOldEvent, traceId);
                    return;
                default: return;
            }
        }

        private void ShowBetweenNodes(AccidentAsNewEvent accidentAsNewEvent, Guid traceId)
        {
            var accidentGps = _accidentPlaceLocator.GetAccidentGps(accidentAsNewEvent);
            if (accidentGps == null) return;

           AddAccidentNode((PointLatLng)accidentGps, traceId, accidentAsNewEvent.AccidentSeriousness);
        }

        private void ShowInNode(AccidentInOldEvent accidentInOldEvent, Guid traceId)
        {
            var nodeVm = _model.GetNodeByLandmarkIndex(traceId, accidentInOldEvent.BrokenLandmarkIndex);

            if (nodeVm == null) return;

            AddAccidentNode(nodeVm.Position, traceId, accidentInOldEvent.AccidentSeriousness);
        }

        private void AddAccidentNode(PointLatLng accidentGps, Guid traceId, FiberState state)
        {
            var accidentNode = new NodeVm()
            {
                Id = Guid.NewGuid(),
                Position = accidentGps,
                Type = EquipmentType.AccidentPlace,
                AccidentOnTraceVmId = traceId,
                State = state,
            };
            _model.Data.Nodes.Add(accidentNode);
        }

        private void ShowBadSegment(AccidentInOldEvent accidentInOldEvent, Guid traceId)
        {
            var fiberVm = _model.GetFiberByLandmarkIndexes(traceId, accidentInOldEvent.BrokenLandmarkIndex - 1,
                accidentInOldEvent.BrokenLandmarkIndex);

            fiberVm.AddBadSegment(traceId);
        }

    }
}