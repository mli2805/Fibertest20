using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;

namespace Iit.Fibertest.RtuMngr;

public partial class RtuManager
{
    public async Task<OtauAttachedDto> AttachOtau(AttachOtauDto param)
    {
        _logger.TimestampWithoutMessage(Logs.RtuManager);
        OtauAttachedDto result;

        var newCharon = await _mainCharon.AttachOtauToPort(param.NetAddress, param.OpticalPort);
        if (newCharon != null)
        {
            _logger.Info(Logs.RtuManager,
                $"Otau {param.NetAddress.ToStringA()} attached to port {param.OpticalPort} and has {newCharon.OwnPortCount} ports");
            result = new OtauAttachedDto(ReturnCode.OtauAttachedSuccessfully)
            {
                IsAttached = true,
                OtauId = param.OtauId,
                RtuId = param.RtuId,
                Serial = newCharon.Serial,
                PortCount = newCharon.OwnPortCount,
            };
        }
        else
            result = new OtauAttachedDto(ReturnCode.RtuAttachOtauError)
            {
                ErrorMessage = _mainCharon.LastErrorMessage
            };

        _logger.Info(Logs.RtuManager,
            $"Now RTU has {_mainCharon.OwnPortCount}/{_mainCharon.FullPortCount} ports");
        return result;
    }

    public async Task<OtauDetachedDto> DetachOtau(DetachOtauDto param)
    {
        _logger.TimestampWithoutMessage(Logs.RtuManager);
        OtauDetachedDto result;

        if (await _mainCharon.DetachOtauFromPort(param.OpticalPort))
        {
            _logger.Info(Logs.RtuManager,
                $"Otau {param.NetAddress.ToStringA()} detached from port {param.OpticalPort}");
            _logger.Info(Logs.RtuManager,
                $"Now RTU has {_mainCharon.OwnPortCount}/{_mainCharon.FullPortCount} ports");

            result = new OtauDetachedDto(ReturnCode.OtauDetachedSuccessfully) { IsDetached = true };
        }
        else
        {
            result = new OtauDetachedDto(ReturnCode.RtuDetachOtauError)
            {
                ErrorMessage = _mainCharon.LastErrorMessage
            };
        }

        return result;
    }
}