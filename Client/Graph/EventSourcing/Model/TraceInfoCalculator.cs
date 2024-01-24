using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Graph
{
    public static class TraceInfoCalculator
    {
        public static List<TraceInfoTableItem> CalculateEquipment(Dictionary<EquipmentType, int> dict)
        {
            var rows = new List<TraceInfoTableItem>() { new TraceInfoTableItem(@"RTU", 1) };

            var part = dict.Where(item => item.Key > EquipmentType.CableReserve).ToDictionary(x => x.Key, x => x.Value);
            rows.AddRange(part.Select(item => new TraceInfoTableItem(item.Key.ToLocalizedString(), item.Value)));
            rows.Add(new TraceInfoTableItem(Resources.SID_Equipment__including_RTU, part.Values.Sum() + 1));

            return rows;
        }

        public static List<TraceInfoTableItem> CalculateEquipmentForWeb(Dictionary<EquipmentType, int> dict)
        {
            var rows = new List<TraceInfoTableItem>() { new TraceInfoTableItem(EquipmentType.Rtu.ToSid(), 1) };

            var part = dict.Where(item => item.Key > EquipmentType.CableReserve).ToDictionary(x => x.Key, x => x.Value);
            rows.AddRange(part.Select(item => new TraceInfoTableItem(item.Key.ToSid(), item.Value)));
            rows.Add(new TraceInfoTableItem(@"SID_Equipment__including_RTU", part.Values.Sum() + 1));

            return rows;
        }


        public static List<TraceInfoTableItem> CalculateNodes(Dictionary<EquipmentType, int> dict)
        {
            var rows = new List<TraceInfoTableItem>();

            if (dict.TryGetValue(EquipmentType.EmptyNode, out var value))
                rows.Add(new TraceInfoTableItem(Resources.SID_Node_without_equipment, value));
            if (dict.TryGetValue(EquipmentType.CableReserve, out var value1))
                rows.Add(new TraceInfoTableItem(EquipmentType.CableReserve.ToLocalizedString(), value1));

            var nodeCount = dict.Where(item => item.Key > EquipmentType.AdjustmentPoint).ToDictionary(x => x.Key, x => x.Value).Values.Sum() + 1;
            rows.Add(new TraceInfoTableItem(Resources.SID_In_total__including_RTU, nodeCount));

            return rows;
        }

        public static List<TraceInfoTableItem> CalculateNodesForWeb(Dictionary<EquipmentType, int> dict)
        {
            var rows = new List<TraceInfoTableItem>();

            if (dict.TryGetValue(EquipmentType.EmptyNode, out var value))
                rows.Add(new TraceInfoTableItem(@"SID_Node_without_equipment", value));
            if (dict.TryGetValue(EquipmentType.CableReserve, out var value1))
                rows.Add(new TraceInfoTableItem(EquipmentType.CableReserve.ToSid(), value1));

            var nodeCount = dict.Where(item => item.Key > EquipmentType.AdjustmentPoint).ToDictionary(x => x.Key, x => x.Value).Values.Sum() + 1;
            rows.Add(new TraceInfoTableItem(@"SID_In_total__including_RTU", nodeCount));

            return rows;
        }

        // when trace is under construction equipment could be passed as list
        public static Dictionary<EquipmentType, int> BuildDictionaryByEquipmentType(this Model readModel, List<Guid> traceEquipments)
        {
            var dict = new Dictionary<EquipmentType, int>();
            foreach (var id in traceEquipments.Skip(1))
            {
                dict.Update(readModel.Equipments.First(e => e.EquipmentId == id).Type);
            }
            return dict;
        }

        private static void Update(this Dictionary<EquipmentType, int> dict, EquipmentType type)
        {
            if (dict.ContainsKey(type))
                dict[type]++;
            else dict.Add(type, 1);
        }
    }
}