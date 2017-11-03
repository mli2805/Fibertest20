using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class DcManager
    {
        #region RTU notifies
        public bool ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto monitoringStep)
        {
            return true;
        }

        public bool ProcessRtuChecksChannel(RtuChecksChannelDto dto)
        {
            RtuStation rtuStation;
            if (_rtuStations.TryGetValue(dto.RtuId, out rtuStation))
            {
                if (dto.IsMainChannel)
                    rtuStation.PcAddresses.LastConnectionOnMain = DateTime.Now;
                else
                    rtuStation.PcAddresses.LastConnectionOnReserve = DateTime.Now;
                rtuStation.Version = dto.Version;
            }
            return true;
        }

        public bool ProcessMonitoringResult(MonitoringResultDto result)
        {
            _logFile.AppendLine(
                $"Moniresult from RTU {result.RtuId.First6()}. {result.BaseRefType} on {result.OtauPort.OpticalPort} port. " +
                $"Trace state is {result.TraceState}. Sor size is {result.SorData.Length}. {result.TimeStamp:yyyy-MM-dd hh-mm-ss}");

            var filename = $@"c:\temp\sor\{result.RtuId.First6()} {result.TimeStamp:yyyy-MM-dd hh-mm-ss}.sor";
            var fs = File.Create(filename);
            fs.Write(result.SorData, 0, result.SorData.Length);
            fs.Close();

            return true;
        }
        #endregion

     
    }
}
