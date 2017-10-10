using System.Threading.Tasks;
using Iit.Fibertest.WcfServiceForClientInterface;
using Newtonsoft.Json;

namespace Iit.Fibertest.Client
{
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

        public async Task<string> SendCommand(object cmd)
        {
            return await 
                _wcfService.SendCommand(JsonConvert.SerializeObject(
                    cmd, cmd.GetType(), JsonSerializerSettings));
        }
    }
}