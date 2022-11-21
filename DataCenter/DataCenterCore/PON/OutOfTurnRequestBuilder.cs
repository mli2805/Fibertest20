using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class OutOfTurnRequestBuilder
    {
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly string _trapConnectionId;

        public OutOfTurnRequestBuilder(IMyLog logFile, Model writeModel, string trapConnectionId)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _trapConnectionId = trapConnectionId;
        }

        public DoOutOfTurnPreciseMeasurementDto BuildDto(TrapParserResult parsedTrap)
        {
            var relation = FindRelation(parsedTrap);
            if (relation == null) return null;
            var trace = GetTrace(relation);
            return trace == null ? null : BuildOutOfTurnDto(relation, trace);
        }

        private GponPortRelation FindRelation(TrapParserResult res)
        {
            var relation = _writeModel.GponPortRelations.FirstOrDefault(r => r.TceId == res.TceId
                                                                             && r.SlotPosition == res.Slot
                                                                             && r.GponInterface == res.GponInterface);
            if (relation == null)
                _logFile.AppendLine($"There is no relation for gpon interface {res.GponInterface}");
          
            return relation;
        }

        private Trace GetTrace(GponPortRelation relation)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == relation.RtuId);
            if (rtu == null || rtu.MonitoringState != MonitoringState.On)
            {
                _logFile.AppendLine("RTU is in Manual state or not found.");
                return null;
            }

            if (!_writeModel.TryGetTrace(relation.TraceId, out Trace trace))
            {
                _logFile.AppendLine($"There is no trace on gpon interface {relation.GponInterface}");
                return null;
            }

            if (!trace.IsIncludedInMonitoringCycle)
            {
                _logFile.AppendLine("Trace excluded from monitoring cycle");
                return null;
            }

            return trace;
        }

        private DoOutOfTurnPreciseMeasurementDto BuildOutOfTurnDto(GponPortRelation relation, Trace trace)
        {
             var dto = new DoOutOfTurnPreciseMeasurementDto()
            {
                ConnectionId = _trapConnectionId,
                Id = Guid.NewGuid(),
                RtuId = relation.RtuId,
                RtuMaker = relation.RtuMaker,
                PortWithTraceDto = new PortWithTraceDto()
                {
                    OtauPort = relation.OtauPortDto,
                    TraceId = trace.TraceId,
                },
                IsTrapCaused = true,
            };

            _logFile.AppendLine($"Request for trace {trace.Title} created.");
            return dto;
        }


    }
}
