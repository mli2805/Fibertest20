﻿using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using SnmpSharpNet;

namespace Iit.Fibertest.DataCenterCore
{
    public class TrapExecutor
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly TrapParser _trapParser;
        private readonly ID2RWcfManager _d2RWcfManager;
        private readonly RtuStationsRepository _rtuStationsRepository;

        public TrapExecutor(IniFile iniFile, IMyLog logFile, Model writeModel,
            TrapParser trapParser, ID2RWcfManager d2RWcfManager, RtuStationsRepository rtuStationsRepository)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
            _trapParser = trapParser;
            _d2RWcfManager = d2RWcfManager;
            _rtuStationsRepository = rtuStationsRepository;
        }

        public async Task Process(SnmpV2Packet pkt, EndPoint endPoint)
        {
            var ss = endPoint.ToString().Split(':');
            var tce = _writeModel.Tces.FirstOrDefault(o => o.Ip == ss[0]);
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
                                                                                && r.TceSlot == res.Slot
                                                                                && r.GponInterface == res.GponInterface);
            if (relation == null)
            {
                _logFile.AppendLine($"There is no relation for gpon interface {res.GponInterface}");
                return;
            }

            var trace = _writeModel.Traces.FirstOrDefault(t =>t.OtauPort != null &&
                    t.OtauPort.Serial == relation.OtauPort.Serial && t.OtauPort.OpticalPort == relation.OtauPort.OpticalPort);
            if (trace == null)
            {
                _logFile.AppendLine($"There is no trace on gpon interface {res.GponInterface}");
                return;
            }

            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null)
            {
                _logFile.AppendLine("RTU for trace from trap not found (something strange!)");
                return;
            }

            if (rtu.MonitoringState != MonitoringState.On)
            {
                _logFile.AppendLine("RTU is not in automatic monitoring mode, trap ignored.");
                return;
            }
            
            var dto = new DoOutOfTurnPreciseMeasurementDto()
            {
                RtuId = relation.RtuId,
                PortWithTraceDto = new PortWithTraceDto()
                {
                    OtauPort = relation.OtauPort,
                    TraceId = trace.TraceId,
                },
                IsTrapCaused = true,
            };

            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses != null)
                await _d2RWcfManager.SetRtuAddresses(rtuAddresses, _iniFile, _logFile)
                    .DoOutOfTurnPreciseMeasurementAsync(dto);
        }
    }
}
