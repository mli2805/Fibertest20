using System.Threading.Tasks;
using Iit.Fibertest.WcfServiceForClientInterface;
using Newtonsoft.Json;

namespace Iit.Fibertest.Client
{
    // TODO: Find a better place fo the bus.cs file
    public sealed class Bus
    {
        private readonly IWcfServiceForClient _wcfService;

        public Bus(IWcfServiceForClient wcfServiceForClient)
        {
            _wcfService = wcfServiceForClient;
        }

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

        public Task<string> SendCommand(object cmd)
        {
            //throw new NotImplementedException();
            return Task.FromResult(
                _wcfService.SendCommand(JsonConvert.SerializeObject(
                    cmd, cmd.GetType(), JsonSerializerSettings)));
        }
    }
}