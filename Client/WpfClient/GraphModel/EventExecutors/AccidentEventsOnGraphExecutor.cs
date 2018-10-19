using System;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class AccidentEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;
        private readonly Model _readModel;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly CurrentUser _currentUser;
        private readonly AccidentPlaceLocator _accidentPlaceLocator;

        public AccidentEventsOnGraphExecutor(GraphReadModel model, Model readModel, CurrentlyHiddenRtu currentlyHiddenRtu,
            CurrentUser currentUser, AccidentPlaceLocator accidentPlaceLocator)
        {
            _model = model;
            _readModel = readModel;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _currentUser = currentUser;
            _accidentPlaceLocator = accidentPlaceLocator;
        }

        public void ShowMonitoringResult(MeasurementAdded evnt)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == evnt.TraceId);
            if (trace == null || _currentUser.ZoneId != Guid.Empty &&
                                    !trace.ZoneIds.Contains(_currentUser.ZoneId)) return;

            if (_currentlyHiddenRtu.Collection.Contains(trace.RtuId)) return;
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

            fiberVm.SetBadSegment(traceId, accidentInOldEvent.AccidentSeriousness);
        }

    }
}