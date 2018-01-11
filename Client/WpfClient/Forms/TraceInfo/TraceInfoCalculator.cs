using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceInfoCalculator
    {
        private readonly ReadModel _readModel;

        public TraceInfoCalculator(ReadModel readModel)
        {
            _readModel = readModel;
        }

        public List<TraceInfoTableItem> CalculateEquipment(List<Guid> traceEquipments)
        {
            var rows = new List<TraceInfoTableItem>(){ new TraceInfoTableItem(@"RTU", 1) };
            var dict = BuildEquipmentDictionary(traceEquipments);

            rows.AddRange(dict.Select(item => new TraceInfoTableItem(item.Key.ToLocalizedString(), item.Value)));
            rows.Add(new TraceInfoTableItem("Equipment including RTU", dict.Values.Sum() + 1));

            return rows;
        }

        public List<TraceInfoTableItem> CalculateNodes(List<Guid> traceNodes, List<Guid> traceEquipments)
        {
            var nodesAndPoints = traceNodes.Select(i => _readModel.Nodes.FirstOrDefault(n => n.Id == i));
            var adjustmentPointsCount = nodesAndPoints.Count(n => n != null && n.IsAdjustmentPoint);

            var rows = new List<TraceInfoTableItem>();


            var dict = BuildEquipmentDictionary(traceEquipments);
            var cableReserveCount = ExcludeCableReserveFromDictionary(dict);

            rows.AddRange(dict.Select(item => new TraceInfoTableItem(item.Key.ToLocalizedString(), item.Value)));
            rows.Add(new TraceInfoTableItem("Equipment including RTU", dict.Values.Sum() + 1));

            var emptyAndCableReserveCount = traceNodes.Count - dict.Values.Sum() - adjustmentPointsCount;
            var emptyNodesCount = emptyAndCableReserveCount - cableReserveCount;

            if (emptyNodesCount > 0)
                rows.Add(new TraceInfoTableItem(Resources.SID_Without_equipment, emptyNodesCount));
            if (cableReserveCount > 0)
                rows.Add(new TraceInfoTableItem(EquipmentType.CableReserve.ToLocalizedString(), cableReserveCount));

            rows.Add(new TraceInfoTableItem(Resources.SID_In_total__including_RTU, traceNodes.Count - adjustmentPointsCount));



            return rows;
        }

        private int ExcludeCableReserveFromDictionary(Dictionary<EquipmentType, int> dict)
        {
            var cableReserve = 0;
            if (dict.ContainsKey(EquipmentType.CableReserve))
            {
                cableReserve = dict[EquipmentType.CableReserve];
                dict.Remove(EquipmentType.CableReserve);
            }
            return cableReserve;
        }

        private Dictionary<EquipmentType, int> BuildEquipmentDictionary(List<Guid> traceEquipments)
        {
            var dict = new Dictionary<EquipmentType, int>();
            foreach (var id in traceEquipments.Skip(1).Where(e => e != Guid.Empty))
            {
                var type = _readModel.Equipments.First(e => e.Id == id).Type;
                if (type == EquipmentType.CableReserve)
                    continue;
                if (dict.ContainsKey(type))
                    dict[type]++;
                else dict.Add(type, 1);
            }
            return dict;
        }

        // TODO EmptyNode instead of Guid.Empty
        private Dictionary<EquipmentType, int> BuildWithoutEquipmentDictionary(List<Guid> traceEquipments)
        {
            var dict = new Dictionary<EquipmentType, int>();
            foreach (var id in traceEquipments.Skip(1))
            {
                var type = _readModel.Equipments.First(e => e.Id == id).Type;
                if (type == EquipmentType.CableReserve)
                    continue;
                if (dict.ContainsKey(type))
                    dict[type]++;
                else dict.Add(type, 1);
            }
            return dict;
        }
    }
}