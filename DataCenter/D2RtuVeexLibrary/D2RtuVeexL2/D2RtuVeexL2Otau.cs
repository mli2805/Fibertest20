using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<HttpRequestResult> InitializeCascadingScheme(DoubleAddress rtuDoubleAddress, string mainOtauId)
        {
            var scheme = new VeexOtauCascadingScheme()
            {
                rootConnections = new List<RootConnection>(),
                connections = new List<Connection>(),
            };
            var resetResult = await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
            if (resetResult.HttpStatusCode != HttpStatusCode.NoContent)
                return resetResult;

            scheme.rootConnections.Add(new RootConnection()
            {
                inputOtauId = mainOtauId,
                inputOtauPort = 0,
            });
            return await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
        }

        public async Task<HttpRequestResult> DetachOtau(DoubleAddress rtuDoubleAddress, string otauId)
        {
            var requestResult = await _d2RtuVeexLayer1.GetOtauCascadingScheme(rtuDoubleAddress);
            if (requestResult.HttpStatusCode != HttpStatusCode.OK) return requestResult;

            var scheme = JsonConvert.DeserializeObject<VeexOtauCascadingScheme>(requestResult.ResponseJson);
            if (scheme == null) return requestResult; // strange, but...

            var removed = scheme.connections.RemoveAll(c => c.outputOtauId == otauId);
            if (removed > 0)
            {
                var updateSchemeResult = await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
                if (updateSchemeResult.HttpStatusCode != HttpStatusCode.NoContent) return updateSchemeResult;
            }

            return await _d2RtuVeexLayer1.DeleteOtau(rtuDoubleAddress, otauId);
        }

        public async Task<HttpRequestResult> AttachOtau(DoubleAddress rtuDoubleAddress, NewOtau newOtau, int masterOpticalPort)
        {
            var creationResult = await _d2RtuVeexLayer1.CreateOtau(rtuDoubleAddress, newOtau);
            if (creationResult.HttpStatusCode != HttpStatusCode.Created)
                return creationResult;

            var responseParts = creationResult.ResponseJson.Split('/');
            if (responseParts.Length < 2) return creationResult;

            var otauId = responseParts[1];

            var getSchemeResult = await _d2RtuVeexLayer1.GetOtauCascadingScheme(rtuDoubleAddress);
            if (getSchemeResult.HttpStatusCode != HttpStatusCode.OK) return getSchemeResult;

            var scheme = JsonConvert.DeserializeObject<VeexOtauCascadingScheme>(getSchemeResult.ResponseJson);
            if (scheme == null)
            {
                getSchemeResult.ErrorMessage = "Failed to parse cascading scheme!";
                getSchemeResult.HttpStatusCode = HttpStatusCode.ExpectationFailed;
                return getSchemeResult; // strange, but...
            }

            // Supposing Root OTAU was set during first RTU initialization
            var rootConnection = scheme.rootConnections.FirstOrDefault();
            if (rootConnection == null)
            {
                getSchemeResult.ErrorMessage = "Main OTAU is not set!";
                getSchemeResult.HttpStatusCode = HttpStatusCode.ExpectationFailed;
                return getSchemeResult;
            }

            scheme.connections.Add(new Connection()
            {
                inputOtauId = otauId,
                inputOtauPort = 0,
                outputOtauId = rootConnection.inputOtauId,
                outputOtauPort = masterOpticalPort,
            });

            var changeScheme = await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
            if (changeScheme.HttpStatusCode != HttpStatusCode.NoContent)
                return changeScheme;

            return await _d2RtuVeexLayer1.GetOtau(rtuDoubleAddress, otauId);
        }

    }
}
