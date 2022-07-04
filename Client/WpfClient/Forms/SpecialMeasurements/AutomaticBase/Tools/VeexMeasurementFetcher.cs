using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class VeexMeasurementFetcher
    {
        private readonly IMyLog _logFile;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;

        public VeexMeasurementFetcher(IMyLog logFile, IWcfServiceCommonC2D c2DWcfCommonManager)
        {
            _logFile = logFile;
            _c2DWcfCommonManager = c2DWcfCommonManager;
        }

        public async Task<MeasurementCompletedEventArgs> Fetch(Guid rtuId, Trace trace, Guid clientMeasurementId)
        {
            var getDto = new GetClientMeasurementDto()
            {
                RtuId = rtuId,
                VeexMeasurementId = clientMeasurementId.ToString(),
            };
            while (true)
            {
                await Task.Delay(5000);
                var measResult = await _c2DWcfCommonManager.GetClientMeasurementAsync(getDto);

                if (measResult.ReturnCode != ReturnCode.Ok || measResult.VeexMeasurementStatus == @"failed")
                {
                    var firstLine = measResult.ReturnCode != ReturnCode.Ok
                        ? measResult.ReturnCode.GetLocalizedString()
                        : Resources.SID_Failed_to_do_Measurement_Client__;

                    return new MeasurementCompletedEventArgs(
                        MeasurementCompletedStatus.FailedToFetchFromRtu4000,
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
                    return new MeasurementCompletedEventArgs(
                        MeasurementCompletedStatus.MeasurementCompletedSuccessfully, trace, "", measResultWithSorBytes.SorBytes);
                }
            }
        }

    }
}
