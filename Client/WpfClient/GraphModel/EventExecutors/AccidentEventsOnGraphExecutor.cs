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

        public AccidentEventsOnGraphExecutor(GraphReadModel model, Model readModel, CurrentlyHiddenRtu currentlyHiddenRtu,
            CurrentUser currentUser)
        {
            _model = model;
            _readModel = readModel;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _currentUser = currentUser;
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

        private void ShowAccidentPlaceOnTrace(AccidentOnTraceV2 accident, Guid traceId)
        {
            if (accident.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff)
                ShowBadSegment(accident, traceId);
            else
                ShowPoint(accident, traceId);
        }

        private void ShowPoint(AccidentOnTraceV2 accidentInPoint, Guid traceId)
        {
            AddAccidentNode(accidentInPoint.AccidentCoors, traceId, accidentInPoint.AccidentSeriousness);
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

        private void ShowBadSegment(AccidentOnTraceV2 accidentInOldEvent, Guid traceId)
        {
            var fiberVm = _model.GetFiberByLandmarkIndexes(traceId, accidentInOldEvent.AccidentLandmarkIndex - 1,
                accidentInOldEvent.AccidentLandmarkIndex);

            fiberVm.SetBadSegment(traceId, accidentInOldEvent.AccidentSeriousness);
        }

    }
}