using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Iit.Fibertest.Utils35
{
    public class Logger
    {
        private readonly StreamWriter _logFile;

        public Logger(string filename)
        {
            var logFullFileName = LogFullFileName(filename);
            _logFile = File.AppendText(logFullFileName);
            _logFile.AutoFlush = true;
        }

        private string LogFullFileName(string filename)
        {
            try
            {
                string logFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\log\"));
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                var logFileName = Path.GetFullPath(Path.Combine(logFolder, filename));
                if (!File.Exists(logFileName))
                    using (FileStream fs = File.Create(logFileName))
                    {   // BOM
                        fs.WriteByte(239); fs.WriteByte(187); fs.WriteByte(191);
                    } 
                return logFileName;
            }
            catch (COMException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public void EmptyLine(char ch = ' ')
        {
            string message = new string(ch, 78);
            _logFile.WriteLine(message);
        }
        public void AppendLine(string message)
        {
            message = message.Replace("\0", string.Empty);
            message = message.Trim();
            message = message.Replace("\r\n", " <NL> ");
            _logFile.WriteLine(DateTime.Now + "  " + message.Trim());
        }
        public void Append(string message)
        {
            _logFile.Write(message);
        }
    }
}
