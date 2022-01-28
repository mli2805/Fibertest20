using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        private async Task<HttpRequestResult> GetOtauList(DoubleAddress rtuDoubleAddress)
        {
            var res = await _d2RtuVeexLayer1.GetOtaus(rtuDoubleAddress);
            if (!res.IsSuccessful) return res;

            var otaus = (LinkList)res.ResponseObject;
            var otauList = new List<VeexOtau>();
            foreach (var link in otaus.items)
            {
                var resOtau = await _d2RtuVeexLayer1.GetOtau(rtuDoubleAddress, link.self);
                if (!resOtau.IsSuccessful) return resOtau;
                otauList.Add((VeexOtau)resOtau.ResponseObject);
            }

            res.ResponseObject = otauList;
            return res;
        }

        private async Task<HttpRequestResult> SetRootOtauIntoScheme(DoubleAddress rtuDoubleAddress, string mainOtauId)
        {
            var scheme = new VeexOtauCascadingScheme(mainOtauId);
            return await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
        }

        private async Task<HttpRequestResult> ResetCascadingScheme(DoubleAddress rtuDoubleAddress)
        {
            var scheme = new VeexOtauCascadingScheme();
            return await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
        }
    }
}
