using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace DbMigrationWpf
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
                    Title = parts[4],
                    Comment = parts[5],
                });

            var otauPortDto = GetOtauPort(int.Parse(parts[2]), int.Parse(parts[3]));
            if (otauPortDto.OpticalPort != -1)
                _graphModel.TraceEventsUnderConstruction.Add(
                    new AttachTrace()
                    {
                        TraceId = traceGuid,
                        OtauPortDto = otauPortDto,
                        PreviousTraceState = FiberState.Unknown,
                        AccidentsInLastMeasurement = null,
                    });
        }

        private OtauPortDto GetOtauPort(int rtuId, int oldPortNumber)
        {
            var rtuNode = _graphModel.NodesDictionary[rtuId];
            var rtuGuid = _graphModel.NodeToRtuDictionary[rtuNode];
            var rtuCommand = _graphModel.RtuCommands.First(c => c.Id == rtuGuid);

            var otauPort = new OtauPortDto();
            if (oldPortNumber <= rtuCommand.FullPortCount)
            {
                otauPort.OtauIp = rtuCommand.OtauNetAddress.Ip4Address;
                otauPort.OtauTcpPort = rtuCommand.OtauNetAddress.Port;
                otauPort.IsPortOnMainCharon = true;
                otauPort.OpticalPort = oldPortNumber;
            }
            else
            {
                var charon15 = _graphModel.Charon15S.First(c =>
                    c.RtuId == rtuId && oldPortNumber >= c.FirstPortNumber && oldPortNumber < c.FirstPortNumber + c.PortCount);
                otauPort.OtauIp = charon15.OtauAddress.Ip4Address;
                otauPort.OtauTcpPort = charon15.OtauAddress.Port;
                otauPort.IsPortOnMainCharon = false;
                otauPort.OpticalPort = oldPortNumber - charon15.FirstPortNumber + 1;
            }

            return otauPort;
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

        public void ParseTraceFibers(string[] parts)
        {
            var traceId = int.Parse(parts[1]);
            var evnt = (AddTrace)_graphModel.TraceEventsUnderConstruction.First(e => e is AddTrace && ((AddTrace)e).TraceId == _graphModel.TracesDictionary[traceId]);
            for (int i = 2; i < parts.Length; i++)
            {
                if (parts[i] == "")
                    continue;
                evnt.FiberIds.Add(_graphModel.FibersDictionary[int.Parse(parts[i])]);
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
                var equipmentGuid = equipmentId == -1
                    ? GetEmptyNodeEquipmentGuid(cmd)
                    : _graphModel.EquipmentsDictionary[equipmentId];
                cmd.EquipmentIds.Add(equipmentGuid);
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