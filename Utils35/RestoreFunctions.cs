using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace Iit.Fibertest.Utils35
{
    public static class RestoreFunctions
    {
        public static void ResetCharonThroughComPort(IniFile iniFile35, Logger35 logger35)
        {
            logger35.AppendLine("Charon RESET");
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
                logger35.AppendLine(e.Message, 2);
                logger35.AppendLine($"Can't open {comPortName}", 2);
                return;
            }
            if (charonLogLevel >= 2)
                logger35.AppendLine($"{comPortName} opened successfully.", 2);

            if (charonLogLevel >= 2)
                logger35.AppendLine($"Now RTS is {serialPort.RtsEnable}", 2);
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            if (charonLogLevel >= 2)
                logger35.AppendLine($"Now RTS is {serialPort.RtsEnable}", 2);
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            if (charonLogLevel >= 2)
                logger35.AppendLine($"Now RTS is {serialPort.RtsEnable}", 2);

            serialPort.Close();

            var pause = iniFile35.Read(IniSection.Charon, IniKey.PauseAfterReset, 5);
            logger35.AppendLine($"Pause after charon reset {pause} seconds...");
            Thread.Sleep(TimeSpan.FromSeconds(pause));
            if (charonLogLevel >= 2)
                logger35.AppendLine("Charon reset finished", 2);
        }

        public static void RebootSystem(Logger35 logger35, int delay)
        {
            logger35.AppendLine($"System reboot in {delay} sec...");
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
                logger35.AppendLine(e.Message);
            }
        }
    }
}