using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class MeasurementInterrupter
    {
        private readonly IMyLog _logFile;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;

        public MeasurementInterrupter(IMyLog logFile, IWcfServiceCommonC2D c2RWcfManager)
        {
            _logFile = logFile;
            _c2RWcfManager = c2RWcfManager;
        }

        public async Task Interrupt(Rtu rtu, string log)
        {
            _logFile.AppendLine($@"Interrupting {log}...");

            var dto = new InterruptMeasurementDto()
            {
                RtuId = rtu.Id,
            };
            await _c2RWcfManager.InterruptMeasurementAsync(dto);
        }
    }
}