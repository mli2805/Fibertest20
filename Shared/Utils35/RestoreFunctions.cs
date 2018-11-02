using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.UtilsLib
{
    public static class RestoreFunctions
    {
        public static void ClearArp(IniFile serviceIni, IMyLog serviceLog, IMyLog rtuLog)
        {
            var logLevel = serviceIni.Read(IniSection.General, IniKey.LogLevel, 1);
            var res = Arp.GetTable();
            if (logLevel == 3)
                serviceLog.AppendLine(res);
            Arp.ClearCache();
            rtuLog.AppendLine("Recovery procedure: Clear ARP table.");
            serviceLog.AppendLine("Recovery procedure: Clear ARP table and Reset Charon.");
            res = Arp.GetTable();
            if (logLevel == 3)
                serviceLog.AppendLine(res);
        }

        public static ReturnCode ResetCharonThroughComPort(IniFile iniFile35, IMyLog logFile)
        {
            logFile.EmptyLine();
            logFile.AppendLine("Charon RESET");
            string comPortName = iniFile35.Read(IniSection.Charon, IniKey.ComPort, "COM2");
            int comSpeed = iniFile35.Read(IniSection.Charon, IniKey.ComSpeed, 115200);

            var serialPort = new SerialPort(comPortName, comSpeed);
            try
            {
                serialPort.Open();
            }
            catch (Exception e)
            {
                logFile.AppendLine(e.Message, 2);
                logFile.AppendLine($"Can't open {comPortName}", 2);
                return ReturnCode.CharonComPortError;
            }
            logFile.AppendLine($"{comPortName} opened successfully.", 2);

            logFile.AppendLine($"Now RTS is {serialPort.RtsEnable}", 2);
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            logFile.AppendLine($"Now RTS is {serialPort.RtsEnable}", 2);
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            logFile.AppendLine($"Now RTS is {serialPort.RtsEnable}", 2);

            serialPort.Close();

            var pause = iniFile35.Read(IniSection.Charon, IniKey.PauseAfterReset, 5);
            logFile.AppendLine($"Pause after charon reset {pause} seconds...");
            Thread.Sleep(TimeSpan.FromSeconds(pause));
            logFile.AppendLine("Charon reset finished", 2);
            return ReturnCode.Ok;
        }

        public static void RebootSystem(IMyLog logFile, int delay)
        {
            logFile.AppendLine($"Recovery procedure: System reboot in {delay} sec...");
            ProcessStartInfo proc = new ProcessStartInfo
            {
                FileName = "cmd",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"/C shutdown -f -r -t {delay}"
            };

            try
            {
                Process.Start(proc);
            }
            catch (Exception e)
            {
                logFile.AppendLine(e.Message);
            }
        }
    }
}