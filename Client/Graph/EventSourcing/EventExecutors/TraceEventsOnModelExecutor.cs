using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class TraceEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

      
        public static string AddTrace(this Model model, TraceAdded e)
        {
            Trace trace = Mapper.Map<Trace>(e);
            trace.ZoneIds.Add(model.Zones.First(z => z.IsDefaultZone).ZoneId);
            model.Traces.Add(trace);

            foreach (var fiberId in trace.FiberIds)
                model.Fibers.First(f=>f.FiberId == fiberId).SetState(trace.TraceId, FiberState.NotJoined);
            return null;
        }

        public static string UpdateTrace(this Model model, TraceUpdated source)
        {
            var destination = model.Traces.First(t => t.TraceId == source.Id);
            Mapper.Map(source, destination);
            return null;
        }

        public static string CleanTrace(this Model model, TraceCleaned e)
        {
           var trace = model.Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace == null)
            {
                return $@"TraceCleaned: Trace {e.TraceId} not found";
            }

            var thisTraceActiveMeas = model.ActiveMeasurements.FirstOrDefault(m => m.TraceId == e.TraceId);
            if (thisTraceActiveMeas != null)
                model.ActiveMeasurements.Remove(thisTraceActiveMeas);

            model.RemoveBaseRefsAndMeasurementsForTrace(trace);

            foreach (var fiberId in trace.FiberIds)
            {
                var fiber = model.Fibers.FirstOrDefault(f => f.FiberId == fiberId);
                if (fiber != null)
                {
                    fiber.TracesWithExceededLossCoeff.Remove(trace.TraceId);
                    if (fiber.States.ContainsKey(trace.TraceId))
                        fiber.States.Remove(trace.TraceId);
                }
            }

            //
            // var traceFibers =  model.GetTraceFibersByNodes(trace.NodeIds).ToList();
            // foreach (var fiber in traceFibers)
            // {
            //     fiber.TracesWithExceededLossCoeff.Remove(trace.TraceId);
            //     if (fiber.States.ContainsKey(trace.TraceId))
            //         fiber.States.Remove(trace.TraceId);
            // }

            model.Traces.Remove(trace);
            return null;
        }

        private static void RemoveBaseRefsAndMeasurementsForTrace(this Model model, Trace trace)
        {
            model.Measurements.RemoveAll(m=>m.TraceId == trace.TraceId);
            model.BaseRefs.RemoveAll(b => b.TraceId == trace.TraceId);
        }
      
        public static string RemoveTrace(this Model model, TraceRemoved e)
        {
            var trace = model.Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace == null)
            {
                return $@"TraceRemoved: Trace {e.TraceId} not found";
            }

            var thisTraceActiveMeas = model.ActiveMeasurements.FirstOrDefault(m => m.TraceId == e.TraceId);
            if (thisTraceActiveMeas != null)
                model.ActiveMeasurements.Remove(thisTraceActiveMeas);

            model.RemoveBaseRefsAndMeasurementsForTrace(trace);

            foreach (var fiberId in trace.FiberIds)
            {
                var fiber = model.Fibers.FirstOrDefault(f => f.FiberId == fiberId);
                if (fiber != null)
                {
                    if (model.Traces.Where(t => t.TraceId != e.TraceId).All(t => t.FiberIds.IndexOf(fiber.FiberId) == -1))
                        model.Fibers.Remove(fiber);
                    else
                    {
                        fiber.TracesWithExceededLossCoeff.Remove(trace.TraceId);
                        if (fiber.States.ContainsKey(trace.TraceId))
                            fiber.States.Remove(trace.TraceId);
                    }
                }
            }

            // var traceFibers =  model.GetTraceFibersByNodes(trace.NodeIds).ToList();
            // foreach (var fiber in traceFibers)
            // {
            //     if (model.Traces.Where(t => t.TraceId != e.TraceId).All(t => t.FiberIds.IndexOf(fiber.FiberId) == -1))
            //         model.Fibers.Remove(fiber);
            //     else
            //     {
            //         fiber.TracesWithExceededLossCoeff.Remove(trace.TraceId);
            //         if (fiber.States.ContainsKey(trace.TraceId))
            //             fiber.States.Remove(trace.TraceId);
            //     }
            // }

            foreach (var traceNodeId in trace.NodeIds)
            {
                if (model.Fibers.Any(f => f.NodeId1 == traceNodeId || f.NodeId2 == traceNodeId))
                    continue;

                var node = model.Nodes.FirstOrDefault(n => n.NodeId == traceNodeId); // FirstOrDefault because of possible repetitions in trace
                if (node?.TypeOfLastAddedEquipment != EquipmentType.Rtu)
                    model.Nodes.Remove(node);
            }

            model.Traces.Remove(trace);
            return null;
        }

        public static string AttachTrace(this Model model, TraceAttached e)
        {
            var trace = model.Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace == null)
            {
                return $@"TraceAttached: Trace {e.TraceId} not found";
            }

            trace.Port = e.OtauPortDto.OpticalPort;
            trace.OtauPort = e.OtauPortDto;

            var lastAccidentOnTrace = model.Measurements.LastOrDefault(m =>
                m.TraceId == e.TraceId && m.EventStatus >= EventStatus.EventButNotAnAccident);
            if (lastAccidentOnTrace != null && lastAccidentOnTrace.TraceState != FiberState.Ok)
            {
                model.ActiveMeasurements.Add(lastAccidentOnTrace);
               
            }
            model.ShowMonitoringResult(new MeasurementAdded()
            {
                TraceId = e.TraceId,
                TraceState = e.PreviousTraceState,
                Accidents = e.AccidentsInLastMeasurement,
            });

            return null;
        }

        public static string DetachTrace(this Model model, TraceDetached e)
        {
            var trace = model.Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace == null)
            {
                return $@"TraceDetached: Trace {e.TraceId} not found";
            }

            var thisTraceActiveMeas = model.ActiveMeasurements.FirstOrDefault(m => m.TraceId == e.TraceId);
            if (thisTraceActiveMeas != null)
                model.ActiveMeasurements.Remove(thisTraceActiveMeas);

            model.DetachTrace(trace);
            return null;
        }

        public static void DetachTrace(this Model model, Trace trace)
        {
            trace.Port = -1;
            trace.OtauPort = null;
            trace.IsIncludedInMonitoringCycle = false;
            trace.State = FiberState.NotJoined;

            foreach (var fiberId in trace.FiberIds)
                model.Fibers.FirstOrDefault(f => f.FiberId == fiberId)?
                    .SetState(trace.TraceId, FiberState.NotJoined);

            // foreach (var fiber in  model.GetTraceFibersByNodes(trace.NodeIds))
                // fiber.SetState(trace.TraceId, FiberState.NotJoined);

            model.CleanAccidentPlacesOnTrace(trace.TraceId);
        }
    }

}