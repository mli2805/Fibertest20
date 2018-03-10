using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DbMigrator
{
    public class FileStringTraceParser
    {
        private readonly Graph _graph;

        public FileStringTraceParser(Graph graph)
        {
            _graph = graph;
        }

        public void ParseTrace(string[] parts)
        {
            var traceId = int.Parse(parts[1]);
            var traceGuid = Guid.NewGuid();
            _graph.TracesDictionary.Add(traceId, traceGuid);

            _graph.TraceEventsUnderConstruction.Add(
                new AddTrace()
                {
                    Id = traceGuid,
                    Title = parts[3],
                    Comment = parts[4],
                });

            var port = int.Parse(parts[2]);
            _graph.TracePorts.Add(traceGuid, port);
        }

        public void ParseTraceNodes(string[] parts)
        {
            var traceId = int.Parse(parts[1]);
            var evnt = (AddTrace)_graph.TraceEventsUnderConstruction.First(e => e is AddTrace && ((AddTrace)e).Id == _graph.TracesDictionary[traceId]);
            for (int i = 2; i < parts.Length; i++)
            {
                if (parts[i] == "")
                    continue;
                evnt.Nodes.Add(_graph.NodesDictionary[int.Parse(parts[i])]);
            }
        }

        public void ParseTraceEquipments(string[] parts)
        {
            var traceId = int.Parse(parts[1]);
            var cmd = (AddTrace)_graph.TraceEventsUnderConstruction.First(e => e is AddTrace && ((AddTrace)e).Id == _graph.TracesDictionary[traceId]);
            var rtuGuid = _graph.NodeToRtuDictionary[_graph.NodesDictionary[int.Parse(parts[2])]];
            cmd.RtuId = rtuGuid;
            cmd.Equipments.Add(rtuGuid);
            for (int i = 3; i < parts.Length; i++)
            {
                if (parts[i] == "")
                    continue;
                var equipmentId = int.Parse(parts[i]);
                var equipmetnGuid = equipmentId == -1 
                    ? GetEmptyNodeEquipmentGuid(cmd)
                    : _graph.EquipmentsDictionary[equipmentId];
                cmd.Equipments.Add(equipmetnGuid);
            }



            var traceGuid = _graph.TracesDictionary[traceId];
            var port = _graph.TracePorts[traceGuid];
            if (port != -1)
            {
                var rtu = _graph.RtuCommands.First(c => c.Id == rtuGuid);

                _graph.TraceEventsUnderConstruction.Add(
                    new AttachTrace()
                    {
                        TraceId = traceGuid,
                        OtauPortDto = new OtauPortDto()
                        {
                            OtauIp = rtu.OtauNetAddress.Ip4Address,
                            OtauTcpPort = rtu.OtauNetAddress.Port,
                            OpticalPort = port,
                            IsPortOnMainCharon = true
                        }
                    });
            }
        }

        private Guid GetEmptyNodeEquipmentGuid(AddTrace cmd)
        {
            var index = cmd.Equipments.Count;
            var nodeGuid = cmd.Nodes[index];
            return _graph.EmptyNodes[nodeGuid];
        }
    }
}