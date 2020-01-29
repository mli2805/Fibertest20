﻿using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForC2RInterface;

namespace Iit.Fibertest.Client
{
    public class OnDemandMeasurement
    {
        private readonly IMyLog _logFile;
        private readonly Model _model;
        private readonly IWcfServiceForC2R _c2RWcfManager;

        public OnDemandMeasurement(IMyLog logFile, Model model, IWcfServiceForC2R c2RWcfManager)
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
                ShouldMonitoringBeStopped = false,
            };
            await _c2RWcfManager.InitializeRtuAsync(dto);
        }
    }
}