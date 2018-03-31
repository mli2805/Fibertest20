using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class OnDemandMeasurement
    {
        private readonly IMyLog _logFile;
        private readonly IModel _model;
        private readonly IWcfServiceForClient _c2DWcfManager;

        public OnDemandMeasurement(IMyLog logFile, IModel model, IWcfServiceForClient c2DWcfManager)
        {
            _logFile = logFile;
            _model = model;
            _c2DWcfManager = c2DWcfManager;
        }

        public async Task Interrupt(RtuLeaf rtuLeaf, string log)
        {
            _logFile.AppendLine($@"Interrupting {log}...");
            var rtu = _model.Rtus.FirstOrDefault(r => r.Id == rtuLeaf.Id);
            if (rtu == null) return;

            var dto = new InitializeRtuDto()
            {
                RtuId = rtuLeaf.Id,
                RtuAddresses = new DoubleAddress() { Main = rtu.MainChannel, HasReserveAddress = rtu.IsReserveChannelSet, Reserve = rtu.ReserveChannel },
                ShouldMonitoringBeStopped = false,
            };
            await _c2DWcfManager.InitializeRtuAsync(dto);
        }
    }
}