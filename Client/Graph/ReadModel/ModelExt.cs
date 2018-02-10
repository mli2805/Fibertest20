﻿using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph.Algorithms.ToolKit;

namespace Iit.Fibertest.Graph
{
    public static class ModelExt
    {
        public static TraceModelForBaseRef GetTraceComponentsByIds(this IModel model, Trace trace)
        {
            var nodes = model.GetTraceNodes(trace).ToArray();
            var equipList =
                new List<Equipment>() { new Equipment() { Type = EquipmentType.Rtu } }; // fake RTU, just for indexes match
            equipList.AddRange(model.GetTraceEquipments(trace).ToList()); // without RTU
            var fibers = model.GetTraceFibers(trace).ToArray();

            return new TraceModelForBaseRef
            {
                NodeArray = nodes,
                EquipArray = equipList.ToArray(),
                FiberArray = fibers,
            };

        }

        public static IEnumerable<Fiber> GetTraceFibers(this IModel model, Trace trace)
        {
            return model.GetFibersByNodes(trace.Nodes).Select(i => model.Fibers.Single(f=>f.Id == i));
        }

        public static IEnumerable<Guid> GetFibersByNodes(this IModel model, List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
                yield return GetFiberByNodes(model, nodes[i - 1], nodes[i]);
        }

        private static Guid GetFiberByNodes(this IModel model, Guid node1, Guid node2)
        {
            return model.Fibers.First(
                f => f.Node1 == node1 && f.Node2 == node2 ||
                     f.Node1 == node2 && f.Node2 == node1).Id;
        }

        public static IEnumerable<Trace> GetTracesPassingFiber(this IModel model, Guid fiberId)
        {
            foreach (var trace in model.Traces)
            {
                if (model.GetFibersByNodes(trace.Nodes).Contains(fiberId))
                    yield return trace;
            }
        }

        public static IEnumerable<Node> GetTraceNodes(this IModel model, Trace trace)
        {
            try
            {
                return trace.Nodes.Select(i => model.Nodes.Single(eq => eq.Id == i));
            }
            catch (Exception e)
            {
                model.LogFile.AppendLine(e.Message);
                return null;
            }

        }

        public static IEnumerable<Equipment> GetTraceEquipments(this IModel model, Trace trace)
        {
            try
            {
                return trace.Equipments.Skip(1).Select(i => model.Equipments.Single(eq => eq.Id == i));
            }
            catch (Exception e)
            {
                model.LogFile.AppendLine(e.Message);
                return null;
            }

        }
    }
}