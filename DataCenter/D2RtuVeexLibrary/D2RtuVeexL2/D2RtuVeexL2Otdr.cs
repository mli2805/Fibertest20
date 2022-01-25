using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<HttpRequestResult> InitializeOtdrs(DoubleAddress rtuDoubleAddress)
        {
            var getResult = await _d2RtuVeexLayer1.GetOtdrs(rtuDoubleAddress);
            if (!getResult.IsSuccessful)
                return getResult;

            var otdrs = (LinkList)getResult.ResponseObject;
            string otdrId = otdrs.items.Count == 0 ? "" : otdrs.items[0].self.Split('/')[1];

            var resetResult = await ResetOtdrs(rtuDoubleAddress, otdrId);
            if (!resetResult.IsSuccessful)
                return resetResult;

            return await GetOtdrSettings(rtuDoubleAddress);
        }

        private async Task<HttpRequestResult> ResetOtdrs(DoubleAddress rtuDoubleAddress, string otdrId)
        {
            var startResetResult = await _d2RtuVeexLayer1.ResetOtdr(rtuDoubleAddress, otdrId);

            if (!startResetResult.IsSuccessful)
                return startResetResult;

            string requestStatus;
            do
            {
                await Task.Delay(2000);
                var resetStatus = await _d2RtuVeexLayer1.GetResetOtdrStatus(rtuDoubleAddress, startResetResult.ResponseJson);
                if (!resetStatus.IsSuccessful)
                    return resetStatus;
                var response = resetStatus.ResponseObject as OtdrResetResponse;
                if (response == null)
                    return new HttpRequestResult() { IsSuccessful = false, ErrorMessage = "Failed to reset otdrs" };
                requestStatus = response.status;

            } while (requestStatus != "processed");

            return new HttpRequestResult() { IsSuccessful = true };
        }

        private async Task<HttpRequestResult> GetOtdrSettings(DoubleAddress rtuDoubleAddress)
        {
            var res = await _d2RtuVeexLayer1.GetOtdrs(rtuDoubleAddress);
            if (!res.IsSuccessful)
                return res;

            var otdrs = (LinkList)res.ResponseObject;
            if (otdrs.items.Count == 0)
            {
                res.IsSuccessful = false;
                res.ErrorMessage = "OTDR not found";
                return res;
            }

            var otdrRes = await _d2RtuVeexLayer1.GetOtdr(rtuDoubleAddress, otdrs.items[0].self);
            if (!otdrRes.IsSuccessful)
                return otdrRes;
            var otdr = (VeexOtdr)otdrRes.ResponseObject;
            if (!otdr.isConnected)
            {
                otdrRes.IsSuccessful = false;
                otdrRes.ErrorMessage = "Failed to connect OTDR";
            }

            return otdrRes;
        }
    }
}
