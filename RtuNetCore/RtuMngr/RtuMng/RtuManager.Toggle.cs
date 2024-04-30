using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.UtilsNetCore;

namespace Iit.Fibertest.RtuMngr;

public partial class RtuManager
{
    private readonly List<DamagedOtau> _damagedOtaus = new List<DamagedOtau>();
    private async Task<bool> ToggleToPort(MonitoringPort monitoringPort)
    {
        var cha = monitoringPort.IsPortOnMainCharon
            ? _mainCharon
            : _mainCharon.GetBopCharonWithLogging(monitoringPort.CharonSerial);

        var damagedAddress = cha != null ? cha.NetAddress.Ip4Address : monitoringPort.CharonAddress.Ip4Address;

        // TCP port here is not important
        var damagedOtau = _damagedOtaus.FirstOrDefault(b => b.Ip == damagedAddress);
        if (damagedOtau != null)
        {
            _logger.Info(Logs.RtuManager, $"Port is on damaged BOP {damagedOtau.Ip}");
            if (DateTime.Now - damagedOtau.RebootStarted < _mikrotikRebootTimeout)
            {
                _logger.Info(Logs.RtuManager, $"Mikrotik {damagedAddress} is rebooting, step to the next port");
                return false;
            }
            else
            {
                await InitializeOtau(new RtuInitializedDto());
                cha = monitoringPort.IsPortOnMainCharon
                    ? _mainCharon
                    : _mainCharon.GetBopCharonWithLogging(monitoringPort.CharonSerial);
            }
        }
        else if (cha == null)
        {
            // чарон не записан поломанным, и не найден в памяти по серийнику
            damagedOtau = new DamagedOtau(monitoringPort.CharonAddress.Ip4Address,
                monitoringPort.CharonAddress.Port, "");
            _damagedOtaus.Add(damagedOtau);
            await RunAdditionalOtauRecovery(damagedOtau);
            return false;
        }

        _currentStep = CreateStepDto(MonitoringCurrentStep.Toggle, monitoringPort);

        var toggleResult = await _mainCharon.SetExtendedActivePort(monitoringPort.CharonSerial, monitoringPort.OpticalPort);
        switch (toggleResult)
        {
            case CharonOperationResult.Ok:
                {
                    _logger.Info(Logs.RtuManager, "Toggled Ok.");
                    // Here TCP port is important
                    if (damagedOtau != null &&
                        damagedOtau.Ip == cha!.NetAddress.Ip4Address &&
                        damagedOtau.TcpPort == cha.NetAddress.Port)
                    {
                        _logger.Info(Logs.RtuManager, $"OTAU {cha.NetAddress.ToStringA()} recovered");
                        if (damagedOtau.RebootAttempts >= _config.Value.Recovery.MikrotikRebootAttemptsBeforeNotification)
                        {
                            _logger.Info(Logs.RtuManager, "Send notification to server.");
                            var dto = new BopStateChangedDto()
                            {
                                RtuId = _config.Value.General.RtuId,
                                Serial = monitoringPort.CharonSerial,
                                OtauIp = cha.NetAddress.Ip4Address,
                                TcpPort = cha.NetAddress.Port,
                                IsOk = true,
                            };
                            await SaveBopEvent(dto);
                        }
                        _damagedOtaus.Remove(damagedOtau);
                    }

                    return true;
                }
            case CharonOperationResult.MainOtauError:
                {
                    _serialPortManager.ShowOnLedDisplay(LedDisplayCode.ErrorTogglePort);
                    if (await RunMainCharonRecovery() != ReturnCode.Ok)
                        await RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                    return false;
                }
            case CharonOperationResult.AdditionalOtauError:
                {
                    if (damagedOtau == null)
                    {
                        damagedOtau = new DamagedOtau(cha!.NetAddress.Ip4Address, cha.NetAddress.Port, monitoringPort.CharonSerial);
                        _damagedOtaus.Add(damagedOtau);
                    }
                    await RunAdditionalOtauRecovery(damagedOtau);
                    return false;
                }
            default:
                {
                    _logger.Info(Logs.RtuManager, _mainCharon.LastErrorMessage);
                    return false;
                }
        }
    }

}