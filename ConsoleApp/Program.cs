using System;
using System.Collections.Generic;
using System.IO;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;

namespace ConsoleAppOtdr
{
    class Program
    {
        private static OtdrManager _otdrManager;
        private static Logger35 _logger35;

        static void Main(string[] args)
        {
            _logger35 = new Logger35();
            _logger35.AssignFile("");
            _otdrManager = new OtdrManager(@"..\IitOtdr\", _logger35);
            if (_otdrManager.LoadDll() == "")
            {
                string otdrAddress;
                if (args.Length == 1)
                    otdrAddress = args[0];
                else
                {
//                    otdrAddress = "172.16.4.10";
//                    otdrAddress = "192.168.88.101";
                    otdrAddress = "192.168.96.52";
                }
                if (_otdrManager.InitializeLibrary())
                    _otdrManager.ConnectOtdr(otdrAddress);
                if (_otdrManager.IsOtdrConnected)
                {
                    var content = ReadCurrentParameters();
                    File.WriteAllLines(@"c:\temp\paramOtdr.txt", content.ToArray());
                }
            }
            Console.Write("Done.");
            Console.Read();
        }

        private static List<string> ReadCurrentParameters()
        {
            var content = new List<string>();
            content.Add($"Unit = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.Unit)}");
            content.Add($"ActiveUnit = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.ActiveUnit)}");

            content.Add($"ActiveRi = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.ActiveRi)}");

            content.Add($"ActiveBc = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.ActiveBc)}");

            content.Add($"Lmax = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.Lmax)}");
            content.Add($"ActiveLmax = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.ActiveLmax)}");

            content.Add($"Res = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.Res)}");
            content.Add($"ActiveRes = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.ActiveRes)}");

            content.Add($"Pulse = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.Pulse)}");
            content.Add($"ActivePulse = {_otdrManager.IitOtdr.GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.ActivePulse)}");

            var isTime = _otdrManager.IitOtdr.GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.ActiveIsTime);
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