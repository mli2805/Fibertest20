using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class ClientToRtuTransmitter
    {
        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            _logFile.AppendLine($"Client from {dto.ClientIp} sent attach OTAU {dto.OtauId.First6()} request");
            OtauAttachedDto result;
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                {
                    result = await _d2RWcfManager.SetRtuAddresses(rtuAddresses, _iniFile, _logFile).AttachOtauAsync(dto);
                }
                else
                {
                    _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                    result = new OtauAttachedDto() { IsAttached = false, ReturnCode = ReturnCode.NoSuchRtu };
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AttachOtauAsync:" + e.Message);
                result = new OtauAttachedDto() { IsAttached = false, ReturnCode = ReturnCode.RtuAttachOtauError, ErrorMessage = e.Message };
            }

            var message = result.IsAttached ? "OTAU attached successfully" : "Failed to attach OTAU";
            _logFile.AppendLine(message);
            return result;
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            _logFile.AppendLine($"Client from {dto.ClientIp} sent detach OTAU {dto.OtauId.First6()} request");
            OtauDetachedDto result;
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                {
                    result = await _d2RWcfManager.SetRtuAddresses(rtuAddresses, _iniFile, _logFile).DetachOtauAsync(dto);
                }
                else
                {
                    _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                    result = new OtauDetachedDto() { IsDetached = false, ReturnCode = ReturnCode.NoSuchRtu };
                }
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
