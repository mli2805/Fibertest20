using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<HttpRequestResult> DetachOtau(DoubleAddress rtuDoubleAddress, string otauId)
        {
            var requestResult = await _d2RtuVeexLayer1.GetOtauCascading(rtuDoubleAddress);
            if (requestResult.HttpStatusCode != HttpStatusCode.OK) return requestResult;

            var scheme = JsonConvert.DeserializeObject<VeexOtauCascadingScheme>(requestResult.ResponseJson);
            if (scheme == null) return requestResult; // strange, but...

            var removed = scheme.connections.RemoveAll(c => c.outputOtauId == otauId);
            if (removed > 0)
            {
                var updateSchemeResult = await _d2RtuVeexLayer1.ChangeOtauCascading(rtuDoubleAddress, scheme);
                if (updateSchemeResult.HttpStatusCode != HttpStatusCode.NoContent) return updateSchemeResult;
            }

            return await _d2RtuVeexLayer1.DeleteOtau(rtuDoubleAddress, otauId);
        }

        public async Task<HttpRequestResult> AttachOtau(DoubleAddress rtuDoubleAddress, CreateOtau createOtau, int masterOpticalPort)
        {
            var creationResult = await _d2RtuVeexLayer1.CreateOtau(rtuDoubleAddress, createOtau);
            if (creationResult.HttpStatusCode != HttpStatusCode.Created)
                return creationResult;

            var responseParts = creationResult.ResponseJson.Split('/');
            if (responseParts.Length < 2) return creationResult;

            var otauId = responseParts[1];

            var getSchemeResult = await _d2RtuVeexLayer1.GetOtauCascading(rtuDoubleAddress);
            if (getSchemeResult.HttpStatusCode != HttpStatusCode.OK) return getSchemeResult;

            var scheme = JsonConvert.DeserializeObject<VeexOtauCascadingScheme>(getSchemeResult.ResponseJson);
            if (scheme == null)
            {
                getSchemeResult.ErrorMessage = "Failed to parse cascading scheme!";
                return getSchemeResult; // strange, but...
            }

            // Supposing Root OTAU was set during first RTU initialization
            var rootConnection = scheme.rootConnections.FirstOrDefault();
            if (rootConnection == null)
            {
                getSchemeResult.ErrorMessage = "Main OTAU is not set!";
                return getSchemeResult;
            }

            scheme.connections.Add(new Connection()
            {
                inputOtauId = otauId,
                inputOtauPort = 0,
                outputOtauId = rootConnection.inputOtauId,
                outputOtauPort = masterOpticalPort,
            });

            return await _d2RtuVeexLayer1.ChangeOtauCascading(rtuDoubleAddress, scheme);
        }

    }
}
