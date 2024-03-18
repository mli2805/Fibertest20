using Microsoft.Extensions.Logging;

namespace Iit.Fibertest.UtilsNetCore;

public class DebianServiceManager(ILogger<DebianServiceManager> logger)
{
    /// <summary>
    /// if application is running line in output appears
    /// </summary>
    /// <param name="executiveFileName">Iit.Fibertest.RtuDaemon</param>
    /// <returns></returns>
    public int CheckServiceRunning(string executiveFileName)
    {
        try
        {
            string shCommand = $"/usr/bin/ps -C {executiveFileName} | sed '1 d' ";
            string output = shCommand.GetCommandLineOutput();

            if (string.IsNullOrEmpty(output))
            {
                logger.Info(Logs.WatchDog, $"{executiveFileName} is not found");
                return 0;
            }

            var ss = output.Split(' ').Where(s=>!string.IsNullOrEmpty(s)).ToArray();
            //logger.Debug(Logs.WatchDog,$"{executiveFileName} is running. PID {ss[0]}");
            var pid = int.Parse(ss[0]);
            return pid;
        }
        catch (Exception e)
        {
            logger.Exception(Logs.WatchDog, e,$"Failed to check service {executiveFileName}");
            return -1;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceName">rtu</param>
    /// <param name="command">start or stop</param>
    /// <returns></returns>
    public bool ServiceCommand(string serviceName, string command)
    {
        try
        {
            string shCommand = $"/usr/sbin/service {serviceName} {command} ";
            return shCommand.ExecuteCommandLine();
        }
        catch (Exception e)
        {
            logger.Exception(Logs.WatchDog, e,$"Failed to start service {serviceName}");
            return false;
        }
    }
}
