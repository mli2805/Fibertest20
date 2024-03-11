using System.Diagnostics;
using System.Runtime.InteropServices;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.Extensions.Logging;

namespace Iit.Fibertest.RtuMngr;

public partial class InterOpWrapper(ILogger<InterOpWrapper> logger)
{
    public bool InitDll(string path)
    {
        IntPtr logFile = IntPtr.Zero;
        IntPtr lenUnit = IntPtr.Zero;

        try
        {
            CppImportDecl.DllInit(path, logFile, lenUnit); // там не срабатывает условная компиляция

            // а здесь идет определение системы в runtime поэтому всё работает
            var libFileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "iit_otdr.dll" : "iit_otdr.so";
            var iitOtdrLib = Path.Combine(path, libFileName);

            FileVersionInfo info = FileVersionInfo.GetVersionInfo(iitOtdrLib);
            var creationTime = File.GetLastWriteTime(iitOtdrLib);
            var version = $"{info.FileVersion} built {creationTime:dd/MM/yyyy}";

            logger.Info(Logs.RtuManager, $"{libFileName} {version} loaded successfully.");
        }
        catch (Exception e)
        {
            logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "InterOpWrapper.DllInit: " + e.Message, e);
            return false;
        }
        return true;
    }


    public bool InitOtdr(ConnectionTypes type, string ip, int port)
    {
        int initOtdr;
        try
        {
            initOtdr = CppImportDecl.InitOTDR((int)type, ip, port);
            SetEqualStepsOfMeasurement();

            if (initOtdr == 0)
            {
                var word1 = type == ConnectionTypes.FreePort ? "disconnected" : "connected";
                logger.Info(Logs.RtuManager, $"OTDR {word1} successfully!");
                return true;
            }

        }
        catch (ExternalException e)
        {
            logger.Exception(Logs.RtuManager, e, "InterOpWrapper.InitOTDR");
            return false;
        }
        catch (Exception e)
        {
            logger.Exception(Logs.RtuManager, e, "InterOpWrapper.InitOTDR");
            return false;
        }

        var word = type == ConnectionTypes.FreePort ? "disconnection" : "connection";
        logger.Error(Logs.RtuManager, $"OTDR {word} failed! Error: {initOtdr}");
        if (initOtdr == 805)
            logger.Error(Logs.RtuManager,
                "InterOpWrapper.InitOTDR: 805 - ERROR_COM_OPEN - check otdr address or reboot RTU");
        return false;
    }
}