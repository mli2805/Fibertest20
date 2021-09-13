using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    // ReSharper disable once InconsistentNaming
    public class OnDemandMeasurement
    {
        private readonly IMyLog _logFile;
        private readonly Model _model;
        private readonly IWcfServiceCommonC2D _c2RWcfManager;

        public OnDemandMeasurement(IMyLog logFile, Model model, IWcfServiceCommonC2D c2RWcfManager)
        {
            _logFile = logFile;
            _model = model;
            _c2RWcfManager = c2RWcfManager;
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
                IsFirstInitialization = false,
            };
            await _c2RWcfManager.InitializeRtuAsync(dto);
        }
    }
}