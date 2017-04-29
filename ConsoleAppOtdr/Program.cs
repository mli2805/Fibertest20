using System;
using System.Collections.Generic;
using System.IO;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.Utils35.IniFile;

namespace ConsoleAppOtdr
{
    class Program
    {
        private static OtdrManager _otdrManager;
        private static Logger35 _logger35;
        private static IniFile _iniFile35;

        static void Main()
        {
            _logger35 = new Logger35();
            _logger35.AssignFile("rtu.log");
            Console.WriteLine("see rtu.log");

            _iniFile35 = new IniFile();
            _iniFile35.AssignFile("rtu.ini");

            if (!OtdrInitialization())
                return;

            while (true)
            {
                if (GetMonitoringSettings() == null)
                    break;


            }


            var content = ReadCurrentParameters();
            _logger35.AppendLine(content, 3);

            _logger35.AppendLine("Done.");
            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        private static Queue<int> GetMonitoringSettings()
        {
            var monitoringSettingsFile = Utils.FileNameForSure(@"..\ini\", @"monitoring.que", false);
            var content = File.ReadAllLines(monitoringSettingsFile);
            var mq = new Queue<int>();
            foreach (var str in content)
            {
                int port;
                if (int.TryParse(str, out port))
                    mq.Enqueue(port);
            }
            return mq;
        }

        private static bool OtdrInitialization()
        {
            _otdrManager = new OtdrManager(@"OtdrMeasEngine\", _logger35);
            if (_otdrManager.LoadDll() != "")
                return false;

            var otdrAddress = _iniFile35.Read(IniSection.General, IniKey.OtdrIp, "192.168.88.101");
            if (_otdrManager.InitializeLibrary())
                _otdrManager.ConnectOtdr(otdrAddress);
            return _otdrManager.IsOtdrConnected;
        }

        private static List<string> ReadCurrentParameters()
        {
            var content = new List<string>();
            content.Add($"Unit = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.Unit)}");
            content.Add($"ActiveUnit = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveUnit)}");

            content.Add($"ActiveRi = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveRi)}");

            content.Add($"ActiveBc = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveBc)}");

            content.Add($"Lmax = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.Lmax)}");
            content.Add($"ActiveLmax = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveLmax)}");

            content.Add($"Res = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.Res)}");
            content.Add($"ActiveRes = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveRes)}");

            content.Add($"Pulse = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.Pulse)}");
            content.Add($"ActivePulse = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActivePulse)}");

            var isTime = _otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveIsTime);
            content.Add($"ActiveIsTime = {isTime}");
            if (1 == int.Parse(isTime))
            {
                content.Add($"Time = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.Time)}");
                content.Add($"ActiveTime = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveTime)}");
            }
            else
            {
                content.Add($"Navr = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.Navr)}");
                content.Add($"ActiveNavr = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveNavr)}");
            }

            // and so on...
            return content;
        }
    }
}