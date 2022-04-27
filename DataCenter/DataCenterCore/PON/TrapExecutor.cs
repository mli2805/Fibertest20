using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using SnmpSharpNet;

namespace Iit.Fibertest.DataCenterCore
{
    public class TrapExecutor
    {
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly TrapParser _trapParser;
        private readonly OutOfTurnQueue _outOfTurnQueue;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;

        public TrapExecutor( IMyLog logFile, Model writeModel,
            TrapParser trapParser, OutOfTurnQueue outOfTurnQueue,
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _trapParser = trapParser;
            _outOfTurnQueue = outOfTurnQueue;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;
        }

        public async Task Process(SnmpV2Packet pkt, EndPoint endPoint)
        {
            var ss = endPoint.ToString().Split(':');
            var tce = _writeModel.TcesNew.FirstOrDefault(o => o.Ip == ss[0]);
            if (tce == null)
            {
                _logFile.AppendLine($"Unknown trap source address: {ss[0]}");
                return;
            }

            var res = _trapParser.Parse(pkt, tce);
            if (res == null)
            {
                _logFile.AppendLine("Failed to parse trap");
                return;
            }

            var relation = _writeModel.GponPortRelations.FirstOrDefault(r => r.TceId == res.TceId
                                                                                && r.SlotPosition == res.Slot
                                                                                && r.GponInterface == res.GponInterface);
            if (relation == null)
            {
                _logFile.AppendLine($"There is no relation for gpon interface {res.GponInterface}");
                return;
            }

            var trace = _writeModel.Traces.FirstOrDefault(t =>t.TraceId == relation.TraceId);
            if (trace == null)
            {
                _logFile.AppendLine($"There is no trace on gpon interface {res.GponInterface}");
                return;
            }
            
            var dto = new DoOutOfTurnPreciseMeasurementDto()
            {
                Id = Guid.NewGuid(),
                RtuId = relation.RtuId,
                PortWithTraceDto = new PortWithTraceDto()
                {
                    OtauPort = relation.OtauPortDto,
                    TraceId = trace.TraceId,
                },
                IsTrapCaused = true,
            };

            _outOfTurnQueue.Requests.Enqueue(dto);

            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == dto.RtuId);
            if (rtu == null) return;

            var unused =  rtu.RtuMaker == RtuMaker.IIT
                ? await _clientToRtuTransmitter.DoOutOfTurnPreciseMeasurementAsync(dto)
                : await _clientToRtuVeexTransmitter.DoOutOfTurnPreciseMeasurementAsync(dto);
        }
    }
}
