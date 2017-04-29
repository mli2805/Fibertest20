using System;
using System.Collections.Generic;
using System.IO;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.Utils35.IniFile;

namespace ConsoleAppOtdr
{
    public class Monirer
    {
        private readonly Logger35 _logger35;
        private readonly IniFile _iniFile35;
        private OtdrManager _otdrManager;

        public Monirer(Logger35 logger35, IniFile iniFile35)
        {
            _logger35 = logger35;
            _iniFile35 = iniFile35;
        }

        public bool InitializeOtdr()
        {
            _otdrManager = new OtdrManager(@"OtdrMeasEngine\", _logger35);
            if (_otdrManager.LoadDll() != "")
                return false;

            var otdrAddress = _iniFile35.Read(IniSection.General, IniKey.OtdrIp, "192.168.88.101");
            if (_otdrManager.InitializeLibrary())
                _otdrManager.ConnectOtdr(otdrAddress);
            return _otdrManager.IsOtdrConnected;
        }

        public void MoniPort(int port)
        {
            var basefile = $@"..\PortData\{port}\PreciseFast.sor";
            if (!File.Exists(basefile))
            {
                _logger35.AppendLine($"Can't find fast base for port {port}");
                return;
            }
            var baseBytes = File.ReadAllBytes(basefile);
            _otdrManager.MeasureWithBase(baseBytes);
        }

    }
    class Program
    {
        private static Logger35 _logger35;
        private static IniFile _iniFile35;

        static void Main()
        {
            _logger35 = new Logger35();
            _logger35.AssignFile("rtu.log");
            Console.WriteLine("see rtu.log");

            _iniFile35 = new IniFile();
            _iniFile35.AssignFile("rtu.ini");

            var monirer = new Monirer(_logger35, _iniFile35);
            
            if (!monirer.InitializeOtdr())
                return;

            var moniQueue = GetMonitoringSettings();
            while (true)
            {
                var port = moniQueue.Dequeue();
                moniQueue.Enqueue(port);

                if (port == -1)
                    break;

                monirer.MoniPort(port);
            }


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

        private static List<string> ReadCurrentParameters(OtdrManager otdrManager)
        {
            var content = new List<string>();
            content.Add($"Unit = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.Unit)}");
            content.Add($"ActiveUnit = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveUnit)}");

            content.Add($"ActiveRi = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveRi)}");

            content.Add($"ActiveBc = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveBc)}");

            content.Add($"Lmax = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.Lmax)}");
            content.Add($"ActiveLmax = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveLmax)}");

            content.Add($"Res = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.Res)}");
            content.Add($"ActiveRes = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveRes)}");

            content.Add($"Pulse = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.Pulse)}");
            content.Add($"ActivePulse = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActivePulse)}");

            var isTime = otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveIsTime);
            content.Add($"ActiveIsTime = {isTime}");
            if (1 == int.Parse(isTime))
            {
                content.Add($"Time = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.Time)}");
                content.Add($"ActiveTime = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveTime)}");
            }
            else
            {
                content.Add($"Navr = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.Navr)}");
                content.Add($"ActiveNavr = {otdrManager.IitOtdr.GetLineOfVariantsForParam((int)ServiceFunctionFirstParam.ActiveNavr)}");
            }

            // and so on...
            return content;
        }
    }
}