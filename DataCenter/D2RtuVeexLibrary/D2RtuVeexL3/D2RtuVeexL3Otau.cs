using System;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto, DoubleAddress rtuDoubleAddress)
        {
            var newOtau = new NewOtau()
            {
                id = "S2_" + dto.OtauId,
                connectionParameters = new VeexOtauAddress()
                {
                    address = dto.NetAddress.Ip4Address,
                    port = dto.NetAddress.Port,
                }
            };
            var res = await _d2RtuVeexLayer2.AttachOtau(rtuDoubleAddress, newOtau, dto.OpticalPort);

            if (res.IsSuccessful && res.ResponseObject is VeexOtau otau)
            {
                return new OtauAttachedDto()
                {
                    ReturnCode = ReturnCode.OtauAttachedSuccessfully,
                    IsAttached = true,
                    RtuId = dto.RtuId,
                    OtauId = Guid.Parse(otau.id.Substring(3)),
                    PortCount = otau.portCount,
                    Serial = otau.serialNumber,
                };
            }

            return new OtauAttachedDto()
            {
                ReturnCode = ReturnCode.RtuAttachOtauError,
                ErrorMessage = res.ErrorMessage + Environment.NewLine + res.ResponseJson,
            };
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto, DoubleAddress rtuDoubleAddress)
        {
            var res = await _d2RtuVeexLayer2.DetachOtau(rtuDoubleAddress, "S2_" + dto.OtauId);

            if (!res.IsSuccessful && res.HttpStatusCode == HttpStatusCode.NotFound)
                res.IsSuccessful = true;

            var result = new OtauDetachedDto()
            {
                IsDetached = res.IsSuccessful,
                RtuId = dto.RtuId,
                OtauId = dto.OtauId,
                ReturnCode = res.IsSuccessful ? ReturnCode.OtauDetachedSuccessfully : ReturnCode.RtuDetachOtauError,
                ErrorMessage = res.ErrorMessage,
            };
            return result;
        }
    }
}
