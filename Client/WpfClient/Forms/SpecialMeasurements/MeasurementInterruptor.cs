using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class MeasurementInterruptor
    {
        private readonly IMyLog _logFile;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;

        public MeasurementInterruptor(IMyLog logFile, IWcfServiceCommonC2D c2RWcfManager)
        {
            _logFile = logFile;
            _c2RWcfManager = c2RWcfManager;
        }

        public async Task Interrupt(Rtu rtu, string log)
        {
            _logFile.AppendLine($@"Interrupting {log}...");

            var dto = new InitializeRtuDto()
            {
                RtuId = rtu.Id,
                RtuAddresses = new DoubleAddress() { Main = rtu.MainChannel, HasReserveAddress = rtu.IsReserveChannelSet, Reserve = rtu.ReserveChannel },
                IsFirstInitialization = false,
            };
            await _c2RWcfManager.InitializeRtuAsync(dto);
        }
    }
}