using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class TraceDevReportExt2
    {
        public static List<string> TraceDevReport2(this Model readModel, Trace trace)
        {
            var content = new List<string>();

            for (int i = 1; i < trace.NodeIds.Count; i++)
            {
                var str = $@"{i:000}  {readModel.NodePart(trace, i)}  {readModel.FiberPart(trace, i)}";
                content.Add(str);
            }

            return content;
        }

        private static string NodePart(this Model readModel, Trace trace, int i)
        {
            var node = readModel.Nodes.FirstOrDefault(n => n.NodeId == trace.NodeIds[i]);
            var nodeTitle = node == null ? @"! нет узла !" : (node.Title ?? "").FixedSize(10);
            var nodeStr = $@"{nodeTitle} {trace.NodeIds[i].First6()}   ";

            var equipment = readModel.Equipments.FirstOrDefault(e => e.EquipmentId == trace.EquipmentIds[i]);
            var eqTitle = equipment == null 
                ? @"! нет обор !" 
                : (equipment.Title ?? "").FixedSize(10);
            var eqType = equipment != null ? equipment.Type.ToShortString() : @"! нет обор !";
            return nodeStr + $@"  {eqTitle} {eqType}";
        }

        private static string FiberPart(this Model readModel, Trace trace, int i)
        {
            var fiberBetweenNodes = readModel.Fibers.FirstOrDefault(
                f => f.NodeId1 == trace.NodeIds[i - 1] && f.NodeId2 == trace.NodeIds[i] ||
                     f.NodeId1 == trace.NodeIds[i] && f.NodeId2 == trace.NodeIds[i - 1]);
            var fiberBetween = fiberBetweenNodes != null ? fiberBetweenNodes.FiberId.First6() : @"! нет!";

            var fiberInTable = readModel.Fibers.FirstOrDefault(f => f.FiberId == trace.FiberIds[i-1]);
            var fiberInTableStr = fiberInTable != null ? fiberInTable.FiberId.First6() : @"! нет!";

            return $@"{fiberBetween}   {fiberInTableStr}";
        }

        private static string FixedSize(this string str, int size)
        {
            if (str.Length > size)
                return str.Substring(0, size);
            return str.PadRight(size);
        }
    }
}