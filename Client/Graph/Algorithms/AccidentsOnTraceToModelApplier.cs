using System;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class AccidentsOnTraceToModelApplier
    {

        public static void CleanAccidentPlacesOnTrace(this Model model, Guid traceId)
        {
            var nodes = model.Nodes.Where(n => n.AccidentOnTraceId == traceId).ToList();
            foreach (var node in nodes)
            {
                model.Nodes.Remove(node);
            }

            foreach (var fiber in model.Fibers)
            {
                fiber.RemoveBadSegment(traceId);
            }
        }

        public static void ShowMonitoringResult(this Model model, MeasurementAdded e)
        {
            var trace = model.Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace == null) return;

            if (trace.State == e.TraceState && trace.State == FiberState.Ok)
                return;
            trace.State = e.TraceState;
            foreach (var fiber in model.GetTraceFibers(trace))
                fiber.SetState(trace.TraceId, trace.State);

            model.CleanAccidentPlacesOnTrace(trace.TraceId);

            if (e.TraceState != FiberState.Unknown && e.TraceState != FiberState.Ok && e.TraceState != FiberState.NoFiber)
                e.Accidents.ForEach(a => model.ShowAccidentPlaceOnTrace(a, e.TraceId));
        }

        private static void ShowAccidentPlaceOnTrace(this Model model, AccidentOnTraceV2 accident, Guid traceId)
        {
            if (accident.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff)
                model.ShowBadSegment(accident, traceId);
            else
                model.ShowPoint(accident, traceId);
        }

       
        private static void ShowPoint(this Model model, AccidentOnTraceV2 accidentInPoint, Guid traceId)
        {
            model.AddAccidentNode(accidentInPoint.AccidentCoors, traceId, accidentInPoint.AccidentSeriousness);
        }

        private static void AddAccidentNode(this Model model, PointLatLng accidentGps, Guid traceId, FiberState state)
        {
            var accidentNode = new Node()
            {
                NodeId = Guid.NewGuid(),
                Position = accidentGps,
                TypeOfLastAddedEquipment = EquipmentType.AccidentPlace,
                AccidentOnTraceId = traceId,
                State = state,
            };
            model.Nodes.Add(accidentNode);
        }

        private static void ShowBadSegment(this Model model, AccidentOnTraceV2 accidentInOldEvent, Guid traceId)
        {
            var fibers = model.GetTraceFibersBetweenLandmarks(traceId, accidentInOldEvent.AccidentLandmarkIndex,
                accidentInOldEvent.AccidentLandmarkIndex + 1);

            foreach (var fiberId in fibers)
            {
                var fiber = model.Fibers.First(f => f.FiberId == fiberId);
                fiber.SetBadSegment(traceId, accidentInOldEvent.AccidentSeriousness);
            }
        }
    }
}