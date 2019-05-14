using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceInfoCalculator
    {
        private readonly Model _readModel;

        public TraceInfoCalculator(Model readModel)
        {
            _readModel = readModel;
        }

        public List<TraceInfoTableItem> CalculateEquipment(Dictionary<EquipmentType, int> dict)
        {
            var rows = new List<TraceInfoTableItem>() { new TraceInfoTableItem(@"RTU", 1) };

            var part = dict.Where(item => item.Key > EquipmentType.CableReserve).ToDictionary(x => x.Key, x => x.Value);
            rows.AddRange(part.Select(item => new TraceInfoTableItem(item.Key.ToLocalizedString(), item.Value)));
            rows.Add(new TraceInfoTableItem(Resources.SID_Equipment__including_RTU, part.Values.Sum() + 1));

            return rows;
        }

        public List<TraceInfoTableItem> CalculateNodes(Dictionary<EquipmentType, int> dict)
        {
            var rows = new List<TraceInfoTableItem>();

            if (dict.ContainsKey(EquipmentType.EmptyNode))
                rows.Add(new TraceInfoTableItem(Resources.SID_Node_without_equipment, dict[EquipmentType.EmptyNode]));
            if (dict.ContainsKey(EquipmentType.CableReserve))
                rows.Add(new TraceInfoTableItem(EquipmentType.CableReserve.ToLocalizedString(), dict[EquipmentType.CableReserve]));

            var nodeCount = dict.Where(item => item.Key > EquipmentType.AdjustmentPoint).ToDictionary(x => x.Key, x => x.Value).Values.Sum() + 1;
            rows.Add(new TraceInfoTableItem(Resources.SID_In_total__including_RTU, nodeCount));

            return rows;
        }

        public Dictionary<EquipmentType, int> BuildDictionaryByEquipmentType(List<Guid> traceEquipments)
        {
            var dict = new Dictionary<EquipmentType, int>();
            foreach (var id in traceEquipments.Skip(1))
            {
                dict.Update(_readModel.Equipments.First(e => e.EquipmentId == id).Type);
            }
            return dict;
        }
    }

    public static class DictionaryExt
    {
        public static void Update(this Dictionary<EquipmentType, int> dict, EquipmentType type)
        {
            if (dict.ContainsKey(type))
                dict[type]++;
            else dict.Add(type, 1);
        }
    }
}