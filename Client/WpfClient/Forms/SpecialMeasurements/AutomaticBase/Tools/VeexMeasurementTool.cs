using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class VeexMeasurementTool
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;

        public VeexMeasurementTool(IniFile iniFile, IMyLog logFile, CurrentUser currentUser, Model readModel, IWcfServiceCommonC2D c2DWcfCommonManager)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _currentUser = currentUser;
            _readModel = readModel;
            _c2DWcfCommonManager = c2DWcfCommonManager;
        }

        public async Task<LineParametersDto> GetLineParametersAsync(MeasurementModel model, TraceLeaf traceLeaf)
        {
            var templateModel = model.OtdrParametersTemplatesViewModel.Model;
            var veexMeasOtdrParameters = templateModel.GetVeexMeasOtdrParametersBase(true);
            var dto = traceLeaf.Parent
                .CreateDoClientMeasurementDto(traceLeaf.PortNumber, false, _readModel, model.CurrentUser)
                .SetParams(true, model.AutoAnalysisParamsViewModel.SearchNewEvents,true, null, veexMeasOtdrParameters);

            // with dto.VeexMeasOtdrParameters.measurementType == "auto_skip_measurement" - it is request of line quality
            var startResult = await _c2DWcfCommonManager.StartClientMeasurementAsync(dto);
            if (startResult.ReturnCode != ReturnCode.MeasurementClientStartedSuccessfully)
                return new LineParametersDto(){ReturnCode = startResult.ReturnCode};

            var p = _iniFile.Read(IniSection.Miscellaneous, IniKey.VeexLineParamsTimeoutMs, 15000);
            await Task.Delay(p);
            var getDto = new GetClientMeasurementDto()
            {
                RtuId = dto.RtuId,
                ConnectionId = _currentUser.ConnectionId,
                VeexMeasurementId = startResult.ClientMeasurementId.ToString(),
            };  
            var lineCheckResult = await _c2DWcfCommonManager.GetClientMeasurementAsync(getDto);
            _logFile.AppendLine($@"{lineCheckResult.ReturnCode.ToString()}");
            if (lineCheckResult.ReturnCode != ReturnCode.Ok)
            {
                _logFile.AppendLine(@"Failed to get line parameters");
                return new LineParametersDto(){ReturnCode = lineCheckResult.ReturnCode};
            }

            var cq = lineCheckResult.ConnectionQuality[0];
            _logFile.AppendLine($@"lmax = {cq.lmaxKm:F},  loss = {cq.loss:F},  reflectance = {cq.reflectance:F},  SNR = {cq.snr:F}");
            return new LineParametersDto()
                { ReturnCode = ReturnCode.Ok, ConnectionQuality = lineCheckResult.ConnectionQuality[0] };
        }
        
        public async Task<MeasurementEventArgs> Fetch(Guid rtuId, Trace trace, Guid clientMeasurementId, CancellationTokenSource cts)
        {
            var getDto = new GetClientMeasurementDto()
            {
                RtuId = rtuId,
                ConnectionId = _currentUser.ConnectionId,
                VeexMeasurementId = clientMeasurementId.ToString(),
            };
            while (!cts.IsCancellationRequested)
            {
                var measResult = await _c2DWcfCommonManager.GetClientMeasurementAsync(getDto);

                if (measResult.ReturnCode != ReturnCode.Ok || measResult.VeexMeasurementStatus == @"failed")
                {
                    var firstLine = measResult.ReturnCode != ReturnCode.Ok
                        ? measResult.ReturnCode.GetLocalizedString()
                        : Resources.SID_Failed_to_do_Measurement_Client__;

                    return new MeasurementEventArgs(
                        ReturnCode.FetchMeasurementFromRtu4000Failed,
                        trace,
                        new List<string>() 
                                {
                                    firstLine,
                                    "",
                                    measResult.ErrorMessage,
                                });
                }

                if (measResult.ReturnCode == ReturnCode.Ok && measResult.VeexMeasurementStatus == @"finished")
                {
                    var measResultWithSorBytes = await _c2DWcfCommonManager.GetClientMeasurementSorBytesAsync(getDto);
                    _logFile.AppendLine($@"Fetched measurement {clientMeasurementId.First6()} from VEEX RTU");
                    return new MeasurementEventArgs(
                        ReturnCode.MeasurementEndedNormally, trace, measResultWithSorBytes.SorBytes);
                }

                await Task.Delay(2000);
            }

            _logFile.AppendLine(@"cancellation token received, stop fetching");
            return new MeasurementEventArgs(ReturnCode.MeasurementInterrupted, trace, new List<string>());
        }

    }
}
