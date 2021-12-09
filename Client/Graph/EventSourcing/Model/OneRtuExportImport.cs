using System.Linq;

namespace Iit.Fibertest.Graph
{
    public static class OneRtuExportImport
    {
        public static void AddOneRtuToModel(this Model readModel, Model oneRtuModel)
        {
            if (readModel.Rtus.Any(r => r.Id == oneRtuModel.Rtus.First().Id))
                return;

            readModel.Rtus.Add(oneRtuModel.Rtus.First());

            foreach (var otau in oneRtuModel.Otaus)
            {
                readModel.Otaus.Add(otau);
            }

            foreach (var trace in oneRtuModel.Traces)
            {
                readModel.Traces.Add(trace);
            }

            foreach (var node in oneRtuModel.Nodes)
            {
                if (readModel.Nodes.All(n => n.NodeId != node.NodeId))
                    readModel.Nodes.Add(node);
            }

            foreach (var equipment in oneRtuModel.Equipments)
            {
                if (readModel.Equipments.All(n => n.EquipmentId != equipment.EquipmentId))
                    readModel.Equipments.Add(equipment);
            }

            foreach (var fiber in oneRtuModel.Fibers)
            {
                if (readModel.Fibers.All(n => n.FiberId != fiber.FiberId))
                    readModel.Fibers.Add(fiber);
            }

        }

        public static Model CreateOneRtuModel(this Model readModel, Rtu rtu)
        {
            var oneRtuGraphModel = new Model();
            oneRtuGraphModel.Rtus.Add(rtu);

            foreach (var otau in readModel.Otaus.Where(o => o.RtuId == rtu.Id))
            {
                oneRtuGraphModel.Otaus.Add(otau);
            }

            foreach (var trace in readModel.Traces.Where(t => t.RtuId == rtu.Id))
            {
                foreach (var nodeId in trace.NodeIds)
                {
                    if (oneRtuGraphModel.Nodes.All(n => n.NodeId != nodeId))
                        oneRtuGraphModel.Nodes.Add(readModel.Nodes.First(n => n.NodeId == nodeId));
                }

                for (int i = 1; i < trace.EquipmentIds.Count; i++)
                {
                    if (oneRtuGraphModel.Equipments.All(n => n.EquipmentId != trace.EquipmentIds[i]))
                        oneRtuGraphModel.Equipments.Add(readModel.Equipments.First(n => n.EquipmentId == trace.EquipmentIds[i]));
                }

                foreach (var fiberId in trace.FiberIds)
                {
                    if (oneRtuGraphModel.Fibers.All(n => n.FiberId != fiberId))
                        oneRtuGraphModel.Fibers.Add(readModel.Fibers.First(n => n.FiberId == fiberId));
                }

                oneRtuGraphModel.Traces.Add(trace);
            }


            return oneRtuGraphModel;
        }

    }
}
