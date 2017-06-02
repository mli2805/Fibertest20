using System;
using System.Data;
using System.Messaging;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;


namespace ConsoleAppOtdr
{
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

            _logger35.EmptyLine();
            _logger35.EmptyLine('-');
            _logger35.AppendLine("Application started.");
            
            RestoreFunctions.ResetCharonThroughComPort(_iniFile35, _logger35);

            var rtuManager = new RtuManager(_logger35, _iniFile35);

            var arp = _iniFile35.Read(IniSection.Restore, IniKey.ClearArp, 0);
            if (arp != 0)
            {
                _iniFile35.Write(IniSection.Restore, IniKey.ClearArp, 0);
                rtuManager.ClearArp();
            }

            var reboot = _iniFile35.Read(IniSection.Restore, IniKey.RebootSystem, 0);
            if (reboot != 0)
            {
                _iniFile35.Write(IniSection.Restore, IniKey.RebootSystem, 0);
                RestoreFunctions.RebootSystem(_logger35);
            }

            if (!rtuManager.InitializeOtdr())
            {
                _logger35.AppendLine("Done.");
                return;
            }
            
            if (!rtuManager.InitializeOtau())
                return;

            _iniFile35.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            rtuManager.GetMonitoringQueue();
            rtuManager.GetMonitoringParams();
            
//            rtuManager.RunMonitoringCycle();

            _logger35.AppendLine("Application terminated.");
            Console.WriteLine("Application terminated.");
//            Console.ReadKey();
        }

        
        private static void SendMoniResult(MoniResult moniResult)
        {
//                            var queueName = @".\private$\F20";
            var queueName = @"FormatName:Direct=TCP:192.168.96.8\private$\F20";
            //                var queueName = @"FormatName:Direct=TCP:192.168.96.52\private$\F22";
            //                var queueName = @"FormatName:Direct=OS:opx-lmarholin\private$\F22";

            // if (!MessageQueue.Exists(queueName)) // works only for local machine queue/address format
            //     break;

            using (MessageQueue queue = new MessageQueue(queueName))
            {
                var binaryMessageFormatter = new BinaryMessageFormatter();
                using (var message = new Message(moniResult, binaryMessageFormatter))
                {
                    message.Recoverable = true;
                    queue.Send(message, MessageQueueTransactionType.Single);
//                    queue.Send(message);
                }
            }

        }

      

    }
}