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
        private readonly Model _writeModel;
        private readonly OutOfTurnData _outOfTurnData;

        public TrapExecutor(Model writeModel, OutOfTurnData outOfTurnData)
        {
            _writeModel = writeModel;
            _outOfTurnData = outOfTurnData;
        }

        public async Task Process(SnmpV2Packet pkt, EndPoint endPoint, IMyLog logFile)
        {
            await Task.Delay(1);
            var relation = ParseTrapReturnRelation(pkt, endPoint, logFile);
            if (relation == null) return;

            if (!_writeModel.TryGetTrace(relation.TraceId, out Trace trace))
            {
                logFile.AppendLine($"There is no trace on gpon interface {relation.GponInterface}");
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

            logFile.AppendLine($"Request for trace {trace.Title} created.");
            _outOfTurnData.AddNewRequest(dto, logFile);
       }

        private GponPortRelation ParseTrapReturnRelation(SnmpV2Packet pkt, EndPoint endPoint, IMyLog logFile)
        {
            var ss = endPoint.ToString().Split(':');
            var tce = _writeModel.TcesNew.FirstOrDefault(o => o.Ip == ss[0]);
            if (tce == null)
            {
                logFile.AppendLine($"Unknown trap source address: {ss[0]}");
                return null;
            }

            var res = pkt.Parse(tce, logFile);
            if (res == null)
            {
                logFile.AppendLine("Failed to parse trap (maybe it is not a line event trap)");
                return null;
            }

            var relation = _writeModel.GponPortRelations.FirstOrDefault(r => r.TceId == res.TceId
                                                                             && r.SlotPosition == res.Slot
                                                                             && r.GponInterface == res.GponInterface);
            if (relation == null)
                logFile.AppendLine($"There is no relation for gpon interface {res.GponInterface}");

            return relation;
        }
    }
}
