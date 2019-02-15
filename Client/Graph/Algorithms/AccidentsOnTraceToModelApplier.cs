using System;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public  class AccidentsOnTraceToModelApplier
    {
        private readonly Model _model;

        public AccidentsOnTraceToModelApplier(Model model)
        {
            _model = model;
        }

        public  void CleanAccidentPlacesOnTrace(Guid traceId)
        {
            var nodes = _model.Nodes.Where(n => n.AccidentOnTraceId == traceId).ToList();
            foreach (var node in nodes)
            {
                _model.Nodes.Remove(node);
            }

            foreach (var fiber in _model.Fibers)
            {
                fiber.RemoveBadSegment(traceId);
            }
        }

        public void ShowMonitoringResult(MeasurementAdded e)
        {
            var trace = _model.Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace == null) return;

            trace.State = e.TraceState;
            foreach (var fiber in _model.GetTraceFibers(trace))
                fiber.SetState(trace.TraceId, trace.State);

            CleanAccidentPlacesOnTrace(trace.TraceId);

            if (e.TraceState != FiberState.Ok && e.TraceState != FiberState.NoFiber)
                e.Accidents.ForEach(a => ShowAccidentPlaceOnTrace(a, e.TraceId));
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
            var accidentNode = new Node()
            {
                NodeId = Guid.NewGuid(),
                Position = accidentGps,
                TypeOfLastAddedEquipment = EquipmentType.AccidentPlace,
                AccidentOnTraceId = traceId,
                State = state,
            };
            _model.Nodes.Add(accidentNode);
        }

        private void ShowBadSegment(AccidentOnTraceV2 accidentInOldEvent, Guid traceId)
        {
            var fibers = _model.GetTraceFibersBetweenLandmarks(traceId, accidentInOldEvent.AccidentLandmarkIndex,
                accidentInOldEvent.AccidentLandmarkIndex + 1);

            foreach (var fiberId in fibers)
            {
                var fiber = _model.Fibers.First(f => f.FiberId == fiberId);
                fiber.SetBadSegment(traceId, accidentInOldEvent.AccidentSeriousness);
            }
        }
    }
}