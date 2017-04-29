using System;
using System.Messaging;
using System.Collections.Generic;
using System.IO;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.Utils35.IniFile;


namespace ConsoleAppOtdr
{
    [Serializable]
    public class MoniResult
    {
        public int Port { get; set; }
        public DateTime TimeStamp { get; set; }
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
            
//            if (!monirer.InitializeOtdr())
//                return;

            var moniQueue = GetMonitoringSettings();
            while (true)
            {
                var port = moniQueue.Dequeue();
                moniQueue.Enqueue(port);

                if (port == -1)
                    break;

//                monirer.MoniPort(port);

                if (!MessageQueue.Exists(@".\private$\F20"))
                    break;

                var body = new MoniResult() {Port = port, TimeStamp = DateTime.Now};
                using (MessageQueue queue = new MessageQueue(@".\private$\F20"))
                {
                    var binaryMessageFormatter = new BinaryMessageFormatter();
                    using (var message = new Message(body, binaryMessageFormatter))
                    {
                        message.Recoverable = true;
                        queue.Send(message, $"Port {port}, {DateTime.Now}", MessageQueueTransactionType.Single);
                    }
                }

                break;
            }


            _logger35.AppendLine("Done.");
            Console.WriteLine("Done.");
//            Console.ReadKey();
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