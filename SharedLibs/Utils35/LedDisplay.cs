using System;
using System.IO.Ports;

namespace Iit.Fibertest.Utils35
{
    public static class LedDisplay
    {
        public static void Show(IniFile iniFile35, Logger35 logger35, LedDisplayCode code)
        {
            logger35.AppendLine($"Write <{code.ToString()}> on led display");
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

            byte[] buffer = new byte[] { (byte)code };
            try
            {
                serialPort.Write(buffer, 0, 1);
                serialPort.Close();
            }
            catch (Exception e)
            {
                logger35.AppendLine(e.Message, 2);
                logger35.AppendLine($"Can't send to {comPortName}", 2);
            }
        }
    }
}