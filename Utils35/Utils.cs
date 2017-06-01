using System;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;

namespace Iit.Fibertest.Utils35
{
    public static class Utils
    {
        public static string FileNameForSure(string relativePath, string filename, bool isBoomNeeded)
        {
            try
            {
                string folder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath));
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fullPath = Path.GetFullPath(Path.Combine(folder, filename));
                if (File.Exists(fullPath))
                    return fullPath;
                using (FileStream fs = File.Create(fullPath))
                {
                    if (isBoomNeeded)
                    { fs.WriteByte(239); fs.WriteByte(187); fs.WriteByte(191); }
                }
                return fullPath;
            }
            catch (COMException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static void CharonResetThroughComPort(IniFile iniFile35, Logger35 logger35)
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
            if (charonLogLevel >=2)
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


    }
}