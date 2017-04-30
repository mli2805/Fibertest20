using System;
using System.Messaging;
using System.Collections.Generic;
using System.IO;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.Utils35.IniFile;


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

            var overSeer = new OverSeer(_logger35, _iniFile35);
            
            if (!overSeer.InitializeOtdr())
                return;

            var moniQueue = GetMonitoringSettings();
            while (true)
            {
                var port = moniQueue.Dequeue();
                moniQueue.Enqueue(port);

                if (port == -1)
                    break;

                
                var moniResult = overSeer.MoniPort(port, BaseRefType.Fast);
                SendMoniResult(moniResult);

                break;
            }


            _logger35.AppendLine("Done.");
            Console.WriteLine("Done.");
//            Console.ReadKey();
        }

        private static void SendMoniResult(MoniResult moniResult)
        {
            //                var queueName = @".\private$\F22";
            var queueName = @"FormatName:Direct=TCP:192.168.0.111\private$\F22";
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

    }
}