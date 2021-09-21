using System.Linq;
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
    public class OltTrapExecutor
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly OltTrapParser _oltTrapParser;
        private readonly ID2RWcfManager _d2RWcfManager;
        private readonly RtuStationsRepository _rtuStationsRepository;

        public OltTrapExecutor( IniFile iniFile, IMyLog logFile, Model writeModel,
            OltTrapParser oltTrapParser, ID2RWcfManager d2RWcfManager, RtuStationsRepository rtuStationsRepository)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _writeModel = writeModel;
            _oltTrapParser = oltTrapParser;
            _d2RWcfManager = d2RWcfManager;
            _rtuStationsRepository = rtuStationsRepository;
        }

        public async Task Process(SnmpV2Packet pkt, EndPoint endPoint)
        {
            var ss = endPoint.ToString().Split(':');
            var olt = _writeModel.Olts.FirstOrDefault(o => o.Ip == ss[0]);
            if (olt == null)
            {
                _logFile.AppendLine($"Unknown OLT {ss[0]}");
                return;
            }

            var res = _oltTrapParser.Parse(pkt, olt);
            if (res == null)
            {
                _logFile.AppendLine("Failed to parse trap");
                return;
            }

            var relation = olt.Relations.FirstOrDefault(r => r.GponInterface == res.GponInterface);
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

            var dto = new DoOutOfTurnPreciseMeasurementDto()
            {
                RtuId = relation.RtuId,
                PortWithTraceDto = new PortWithTraceDto()
                {
                    OtauPort = relation.OtauPort,
                    TraceId = trace.TraceId,
                },
                IsOltTrapCaused = true,
            };

            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses != null)
                await _d2RWcfManager.SetRtuAddresses(rtuAddresses, _iniFile, _logFile)
                    .DoOutOfTurnPreciseMeasurementAsync(dto);
        }
    }
}
