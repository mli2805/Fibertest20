using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class ClientToRtuTransmitter
    {
        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto, DoubleAddress rtuDoubleAddress)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} sent attach OTAU {dto.OtauId.First6()} request");
            OtauAttachedDto result;
            try
            {
               
                    result = await _d2RWcfManager.SetRtuAddresses(rtuDoubleAddress, _iniFile, _logFile).AttachOtauAsync(dto);
                
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AttachOtauAsync:" + e.Message);
                result = new OtauAttachedDto() { IsAttached = false, ReturnCode = ReturnCode.RtuAttachOtauError, ErrorMessage = e.Message };
            }

            var message = result == null || !result.IsAttached ? "Failed to attach OTAU" : "OTAU attached successfully";
            _logFile.AppendLine(message);
            return result;
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto, DoubleAddress rtuDoubleAddress)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} sent detach OTAU {dto.OtauId.First6()} request");
            OtauDetachedDto result;
            try
            {
                    result = await _d2RWcfManager.SetRtuAddresses(rtuDoubleAddress, _iniFile, _logFile).DetachOtauAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DetachOtauAsync:" + e.Message);
                result = new OtauDetachedDto() { IsDetached = false, ReturnCode = ReturnCode.RtuDetachOtauError, ErrorMessage = e.Message };
            }

            var message = result.IsDetached ? "OTAU detached successfully" : "Failed to detach OTAU";
            _logFile.AppendLine(message);
            return result;
        }
    }
}
