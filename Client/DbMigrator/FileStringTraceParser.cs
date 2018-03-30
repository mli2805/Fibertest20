using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DbMigrator
{
    public class FileStringTraceParser
    {
        private readonly GraphModel _graphModel;

        public FileStringTraceParser(GraphModel graphModel)
        {
            _graphModel = graphModel;
        }

        public void ParseTrace(string[] parts)
        {
            var traceId = int.Parse(parts[1]);
            var traceGuid = Guid.NewGuid();
            _graphModel.TracesDictionary.Add(traceId, traceGuid);

            _graphModel.TraceEventsUnderConstruction.Add(
                new AddTrace()
                {
                    TraceId = traceGuid,
                    Title = parts[3],
                    Comment = parts[4],
                });

            var port = int.Parse(parts[2]);
            _graphModel.TracePorts.Add(traceGuid, port);
        }

        public void ParseTraceNodes(string[] parts)
        {
            var traceId = int.Parse(parts[1]);
            var evnt = (AddTrace)_graphModel.TraceEventsUnderConstruction.First(e => e is AddTrace && ((AddTrace)e).TraceId == _graphModel.TracesDictionary[traceId]);
            for (int i = 2; i < parts.Length; i++)
            {
                if (parts[i] == "")
                    continue;
                evnt.NodeIds.Add(_graphModel.NodesDictionary[int.Parse(parts[i])]);
            }
        }

        public void ParseTraceEquipments(string[] parts)
        {
            var traceId = int.Parse(parts[1]);
            var cmd = (AddTrace)_graphModel.TraceEventsUnderConstruction.First(e => e is AddTrace && ((AddTrace)e).TraceId == _graphModel.TracesDictionary[traceId]);
            var rtuGuid = _graphModel.NodeToRtuDictionary[_graphModel.NodesDictionary[int.Parse(parts[2])]];
            cmd.RtuId = rtuGuid;
            cmd.EquipmentIds.Add(rtuGuid);
            for (int i = 3; i < parts.Length; i++)
            {
                if (parts[i] == "")
                    continue;
                var equipmentId = int.Parse(parts[i]);
                var equipmetnGuid = equipmentId == -1 
                    ? GetEmptyNodeEquipmentGuid(cmd)
                    : _graphModel.EquipmentsDictionary[equipmentId];
                cmd.EquipmentIds.Add(equipmetnGuid);
            }

            var traceGuid = _graphModel.TracesDictionary[traceId];
            var port = _graphModel.TracePorts[traceGuid];
            if (port != -1)
            {
                var rtu = _graphModel.RtuCommands.First(c => c.Id == rtuGuid);

                _graphModel.TraceEventsUnderConstruction.Add(
                    new AttachTrace()
                    {
                        TraceId = traceGuid,
                        OtauPortDto = new OtauPortDto()
                        {
                            OtauIp = rtu.OtauNetAddress.Ip4Address,
                            OtauTcpPort = rtu.OtauNetAddress.Port,
                            OpticalPort = port,
                            IsPortOnMainCharon = true
                        },
                        PreviousTraceState = FiberState.Unknown,
                        AccidentsInLastMeasurement = null,
                    });
            }
        }

        private Guid GetEmptyNodeEquipmentGuid(AddTrace cmd)
        {
            var index = cmd.EquipmentIds.Count;
            var nodeGuid = cmd.NodeIds[index];
            return _graphModel.EmptyNodes[nodeGuid];
        }
    }
}