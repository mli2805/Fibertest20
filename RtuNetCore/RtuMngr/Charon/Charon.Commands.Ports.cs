using Iit.Fibertest.UtilsNetCore;

namespace Iit.Fibertest.RtuMngr;

public partial class Charon
{
    // public Task<bool> GetExtendedActivePort(out NetAddress charonAddress, out int port)
    // {
    //     var activePort = GetActivePort().WaitAsync();
    //     if (!Children.ContainsKey(activePort))
    //     {
    //         charonAddress = NetAddress;
    //         port = activePort;
    //         return true;
    //     }
    //
    //     var activeCharon = Children[activePort];
    //     return await activeCharon.GetExtendedActivePort(out charonAddress, out port);
    // }

    // public async Task<Charon?> GetActiveChildCharon()
    // {
    //     var activePort = await GetActivePort();
    //     if (!Children.ContainsKey(activePort))
    //     {
    //         return null;
    //     }
    //     return Children[activePort];
    // }

    public async Task<CharonOperationResult> SetExtendedActivePort(string serial, int port)
    {
        _logger.Info(Logs.RtuManager, $"Toggling to port {port} on {serial}...");
        if (Serial == serial)
            return await SetActivePortOnMainCharon(port);
        else
        {
            var bopCharon = GetBopCharonWithLogging(serial);
            if (bopCharon == null)
                return CharonOperationResult.AdditionalOtauError;
            else
            {
                var result = await ToggleMasterCharonToBopIfNeeded(bopCharon);
                return result == CharonOperationResult.Ok ? await SetActivePortOnBopCharon(bopCharon, port) : result;
            }
        }
    }

    private async Task<CharonOperationResult> SetActivePortOnMainCharon(int port)
    {
        var activePort = await SetActivePort(port);
        if (activePort == port)
            return CharonOperationResult.Ok;

        _logger.Info(Logs.RtuManager, "Toggling second attempt...");
        activePort = await SetActivePort(port);
        if (activePort == port)
            return CharonOperationResult.Ok;

        LastErrorMessage = $"Can't toggle switch into {port} port";
        _logger.Error(Logs.RtuManager, LastErrorMessage);
        return CharonOperationResult.MainOtauError;
    }

      
    public Charon? GetBopCharonWithLogging(string serial)
    {
        var charon = Children.Values.FirstOrDefault(c => c.Serial == serial);
        if (charon == null)
        {
            LastErrorMessage = "There is no such optical switch";
            _logger.Error(Logs.RtuManager, LastErrorMessage);
        }
        return charon;
    }

    private async Task<CharonOperationResult> ToggleMasterCharonToBopIfNeeded(Charon charon)
    {
        var masterPort = Children.First(pair => pair.Value == charon).Key;
        return await GetActivePort() != masterPort ? await SetActivePortOnMainCharon(masterPort) : CharonOperationResult.Ok;
    }

    private async Task<CharonOperationResult> SetActivePortOnBopCharon(Charon charon, int port)
    {
        _logger.Debug(Logs.RtuManager, "SetActivePortOnBopCharon");
        var activePort = await charon.SetActivePort(port);
        if (activePort == port)
            return CharonOperationResult.Ok;

        _logger.Info(Logs.RtuManager, "Toggling second attempt...");
        activePort = await charon.SetActivePort(port);
        if (activePort == port)
            return CharonOperationResult.Ok;

        LastErrorMessage = charon.LastErrorMessage;
        IsLastCommandSuccessful = charon.IsLastCommandSuccessful;
        _logger.Error(Logs.RtuManager, LastErrorMessage);
        return CharonOperationResult.AdditionalOtauError;
    }
}