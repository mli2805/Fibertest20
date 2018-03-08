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
            if (port != -1)
                _graph.TraceEventsUnderConstruction.Add(
                    new AttachTrace()
                    {
                        TraceId = traceGuid,
                        OtauPortDto = new OtauPortDto() { OpticalPort = port }
                    });
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
            var evnt = (AddTrace)_graph.TraceEventsUnderConstruction.First(e => e is AddTrace && ((AddTrace)e).Id == _graph.TracesDictionary[traceId]);
            var rtuGuid = _graph.NodeToRtuDictionary[_graph.NodesDictionary[int.Parse(parts[2])]];
            evnt.RtuId = rtuGuid;
            evnt.Equipments.Add(rtuGuid);
            for (int i = 3; i < parts.Length; i++)
            {
                if (parts[i] == "")
                    continue;
                var equipmentId = int.Parse(parts[i]);
                evnt.Equipments.Add(equipmentId == -1 ? Guid.Empty : _graph.EquipmentsDictionary[equipmentId]);
            }
        }

    }
}