using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<HttpRequestResult> DetachOtau(DoubleAddress rtuDoubleAddress, string otauId)
        {
            var getSchemeResult = await _d2RtuVeexLayer1.GetOtauCascadingScheme(rtuDoubleAddress);
            if (!getSchemeResult.IsSuccessful) return getSchemeResult;

            var scheme = (VeexOtauCascadingScheme)getSchemeResult.ResponseObject;

            var removed = scheme.connections.RemoveAll(c => c.inputOtauId == otauId);
            if (removed > 0)
            {
                var updateSchemeResult = await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
                if (!updateSchemeResult.IsSuccessful) return updateSchemeResult;
            }

            return await _d2RtuVeexLayer1.DeleteOtau(rtuDoubleAddress, otauId);
        }

        public async Task<HttpRequestResult> AttachOtau(DoubleAddress rtuDoubleAddress, NewOtau newOtau, int masterOpticalPort)
        {
            var creationResult = await _d2RtuVeexLayer1.CreateOtau(rtuDoubleAddress, newOtau);
            if (!creationResult.IsSuccessful)
                return creationResult;

            var responseParts = creationResult.ResponseJson.Split('/');
            if (responseParts.Length < 2) return creationResult;

            var otauId = responseParts[1];

            var getSchemeResult = await _d2RtuVeexLayer1.GetOtauCascadingScheme(rtuDoubleAddress);
            if (!getSchemeResult.IsSuccessful) return getSchemeResult;

            var scheme = (VeexOtauCascadingScheme)getSchemeResult.ResponseObject;

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
                outputOtauPort = masterOpticalPort - 1,
            });

            var changeScheme = await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
            if (!changeScheme.IsSuccessful)
                return changeScheme;

            return await _d2RtuVeexLayer1.GetOtau(rtuDoubleAddress, creationResult.ResponseJson);
        }
    }
}
