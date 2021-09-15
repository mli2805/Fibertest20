using System;
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
                    ReturnCode = ReturnCode.OtauAttachedSuccesfully,
                    IsAttached = true,
                    RtuId = dto.RtuId,
                    OtauId = Guid.Parse(otau.id),
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
            var res = await _d2RtuVeexLayer2.DetachOtau(rtuDoubleAddress, dto.OtauId.ToString());

            var result = new OtauDetachedDto()
            {
                IsDetached = res.IsSuccessful,
                RtuId = dto.RtuId,
                OtauId = dto.OtauId,
                ReturnCode = res.IsSuccessful ? ReturnCode.OtauDetachedSuccesfully : ReturnCode.RtuDetachOtauError,
                ErrorMessage = res.ErrorMessage,
            };
            return result;
        }
    }
}
