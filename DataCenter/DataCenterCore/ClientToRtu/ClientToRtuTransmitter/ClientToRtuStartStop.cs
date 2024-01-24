using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class ClientToRtuTransmitter
    {
        public async Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto, DoubleAddress rtuDoubleAddress)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} sent monitoring settings for RTU {dto.RtuId.First6()}");
            try
            {
                    var result = await _d2RWcfManager.SetRtuAddresses(rtuDoubleAddress, _iniFile, _logFile)
                        .ApplyMonitoringSettingsAsync(dto);
                    _logFile.AppendLine($"Apply monitoring settings result is {result.ReturnCode}");
                    return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("ApplyMonitoringSettingsAsync:" + e.Message);
                return new RequestAnswer() { ReturnCode = ReturnCode.DbError, ErrorMessage = e.Message };
            }
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} sent stop monitoring on RTU {dto.RtuId.First6()} request");
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                {
                    var result = await _d2RWcfManager.SetRtuAddresses(rtuAddresses, _iniFile, _logFile)
                        .StopMonitoringAsync(dto);
                    _logFile.AppendLine($"Stop monitoring result is {result}");
                    return result;
                }

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return false;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("StopMonitoringAsync:" + e.Message);
                return false;
            }
        }
    }
}
