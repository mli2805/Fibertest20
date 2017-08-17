using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace Iit.Fibertest.UtilsLib
{
    public static class RestoreFunctions
    {
        public static void ResetCharonThroughComPort(IniFile iniFile35, LogFile logFile)
        {
            logFile.AppendLine("Charon RESET");
            string comPortName = iniFile35.Read(IniSection.Charon, IniKey.ComPort, "COM2");
            int comSpeed = iniFile35.Read(IniSection.Charon, IniKey.ComSpeed, 115200);
            int charonLogLevel = iniFile35.Read(IniSection.Charon, IniKey.LogLevel, 4);

            var serialPort = new SerialPort(comPortName, comSpeed);
            try
            {
                serialPort.Open();
            }
            catch (Exception e)
            {
                logFile.AppendLine(e.Message, 2);
                logFile.AppendLine($"Can't open {comPortName}", 2);
                return;
            }
            if (charonLogLevel >= 2)
                logFile.AppendLine($"{comPortName} opened successfully.", 2);

            if (charonLogLevel >= 2)
                logFile.AppendLine($"Now RTS is {serialPort.RtsEnable}", 2);
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            if (charonLogLevel >= 2)
                logFile.AppendLine($"Now RTS is {serialPort.RtsEnable}", 2);
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            if (charonLogLevel >= 2)
                logFile.AppendLine($"Now RTS is {serialPort.RtsEnable}", 2);

            serialPort.Close();

            var pause = iniFile35.Read(IniSection.Charon, IniKey.PauseAfterReset, 5);
            logFile.AppendLine($"Pause after charon reset {pause} seconds...");
            Thread.Sleep(TimeSpan.FromSeconds(pause));
            if (charonLogLevel >= 2)
                logFile.AppendLine("Charon reset finished", 2);
        }

        public static void RebootSystem(LogFile logFile, int delay)
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