using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class VeexMeasurementFetcher
    {
        private readonly IMyLog _logFile;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;

        public VeexMeasurementFetcher(IMyLog logFile, IWindowManager windowManager, IWcfServiceCommonC2D c2DWcfCommonManager)
        {
            _logFile = logFile;
            _windowManager = windowManager;
            _c2DWcfCommonManager = c2DWcfCommonManager;
        }

        public async Task<byte[]> Fetch(Guid rtuId, Guid clientMeasurementId)
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

                    var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                    {
                        firstLine,
                        "",
                        measResult.ErrorMessage,
                    }, 0);
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    return null;
                }

                if (measResult.ReturnCode == ReturnCode.Ok && measResult.VeexMeasurementStatus == @"finished")
                {
                    var measResultWithSorBytes = await _c2DWcfCommonManager.GetClientMeasurementSorBytesAsync(getDto);
                    _logFile.AppendLine($@"Fetched measurement {clientMeasurementId.First6()} from VEEX RTU");
                    return measResultWithSorBytes.SorBytes;
                }
            }
        }

    }
}
