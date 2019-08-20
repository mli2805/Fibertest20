﻿using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForWebProxyInterface;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForWebProxy : IWcfServiceForWebProxy
    {
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;

        public WcfServiceForWebProxy(IMyLog logFile, Model writeModel)
        {
            _logFile = logFile;
            _writeModel = writeModel;
        }

        public async Task<List<string>>GetRtuList()
        {
            await Task.Delay(1);
            _logFile.AppendLine("We are in WcfServiceForWebProxy");
            return _writeModel.Rtus.Select(r => r.Title).ToList();
        }
    }
}