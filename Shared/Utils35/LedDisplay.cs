using System;
using System.IO.Ports;

namespace Iit.Fibertest.UtilsLib
{
    public static class LedDisplay
    {
        public static void Show(IniFile iniFile35, IMyLog logFile, LedDisplayCode code)
        {
            logFile.AppendLine($"Write <{code.ToString()}> on led display");
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

            byte[] buffer = new byte[] { (byte)code };
            try
            {
                serialPort.Write(buffer, 0, 1);
                serialPort.Close();
            }
            catch (Exception e)
            {
                logFile.AppendLine(e.Message, 2);
                logFile.AppendLine($"Can't send to {comPortName}", 2);
            }
        }
    }
}