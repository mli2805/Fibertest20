using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        private async Task<HttpRequestResult> GetOtdrSettings(DoubleAddress rtuDoubleAddress)
        {
            var res = await _d2RtuVeexLayer1.GetOtdrs(rtuDoubleAddress);
            if (!res.IsSuccessful)
                return res;

            var otdrs = (LinkList) res.ResponseObject;
            if (otdrs.items.Count == 0)
                return res;

            return await _d2RtuVeexLayer1.GetOtdr(rtuDoubleAddress, otdrs.items[0].self);
        }
    }
}
