using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<HttpRequestResult> GetOtauSettings(DoubleAddress rtuDoubleAddress)
        {
            var res = await _d2RtuVeexLayer1.GetOtaus(rtuDoubleAddress);
            if (!res.IsSuccessful) return res;

            var otaus = (VeexOtaus) res.ResponseObject;
            var otauList = new List<VeexOtau>();
            foreach (var link in otaus.items)
            {
                var resOtau = await _d2RtuVeexLayer1.GetOtau(rtuDoubleAddress, link.self);
                if (!resOtau.IsSuccessful) return resOtau;
                otauList.Add((VeexOtau)resOtau.ResponseObject);
            }

            var resScheme = await _d2RtuVeexLayer1.GetOtauCascadingScheme(rtuDoubleAddress);
            if (!resScheme.IsSuccessful) return resScheme;

            if (otauList.Count > 0 && ((VeexOtauCascadingScheme) resScheme.ResponseObject).rootConnections.Count == 0)
            {
                // first initialization
                var initSchemeRes = await InitializeCascadingScheme(rtuDoubleAddress, otauList[0].id);
                if (!initSchemeRes.IsSuccessful)
                {
                    initSchemeRes.ErrorMessage = "Failed to set main OTAU as a root in cascading scheme!" 
                                                 + Environment.NewLine + initSchemeRes.ErrorMessage;
                    return initSchemeRes;
                }
            }

            res.ResponseObject = new VeexOtauInfo()
            {
                OtauList = otauList,
                OtauScheme = (VeexOtauCascadingScheme) resScheme.ResponseObject,
            };
            return res;
        }

        private async Task<HttpRequestResult> InitializeCascadingScheme(DoubleAddress rtuDoubleAddress, string mainOtauId)
        {
            var scheme = new VeexOtauCascadingScheme()
            {
                rootConnections = new List<RootConnection>(),
                connections = new List<Connection>(),
            };
            var resetResult = await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
            if (!resetResult.IsSuccessful)
                return resetResult;

            scheme.rootConnections.Add(new RootConnection()
            {
                inputOtauId = mainOtauId,
                inputOtauPort = 0,
            });

            return await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
        }

        public async Task<HttpRequestResult> AdjustCascadingScheme(DoubleAddress rtuDoubleAddress, VeexOtauCascadingScheme scheme)
        {
            return await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
        }

        public async Task<HttpRequestResult> DetachOtau(DoubleAddress rtuDoubleAddress, string otauId)
        {
            var getSchemeResult = await _d2RtuVeexLayer1.GetOtauCascadingScheme(rtuDoubleAddress);
            if (!getSchemeResult.IsSuccessful) return getSchemeResult;

            var scheme = (VeexOtauCascadingScheme)getSchemeResult.ResponseObject;
           
            var removed = scheme.connections.RemoveAll(c => c.outputOtauId == otauId);
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
                outputOtauPort = masterOpticalPort,
            });

            var changeScheme = await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
            if (!changeScheme.IsSuccessful)
                return changeScheme;

            return await _d2RtuVeexLayer1.GetOtau(rtuDoubleAddress, creationResult.ResponseJson);
        }

    }
}
