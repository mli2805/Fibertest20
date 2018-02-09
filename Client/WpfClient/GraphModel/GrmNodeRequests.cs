using System;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class GrmNodeRequests
    {
        private readonly IWcfServiceForClient _c2DWcfManager;

        public GrmNodeRequests(IWcfServiceForClient c2DWcfManager)
        {
            _c2DWcfManager = c2DWcfManager;
        }

    }
}
