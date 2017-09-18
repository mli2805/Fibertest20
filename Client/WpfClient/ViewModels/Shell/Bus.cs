using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WcfConnections.C2DWcfServiceReference;

namespace WcfServiceForClientLibrary
{
    // TODO: Find a better place fo the bus.cs file
    public sealed class Bus
    {
        private readonly IWcfServiceForClient _wcfService;

        public Bus(IWcfServiceForClient wcfServiceForClient)
        {
            _wcfService = wcfServiceForClient;
        }

        public Task<string> SendCommand(object cmd)
        {
            //throw new NotImplementedException();
            return Task.FromResult(
                _wcfService.SendCommand(JsonConvert.SerializeObject(cmd)));
        }
    }
}