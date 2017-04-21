using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Iit.Fibertest.Utils35
{
    public class Logger35
    {
        private StreamWriter _logFile;

        public Logger35()
        {
            
        }

        public Logger35 AssignFile(string filename)
        {
            if (filename != "")
            {
                var logFullFileName = LogFullFileName(filename);
                _logFile = File.AppendText(logFullFileName);
                _logFile.AutoFlush = true;
            }
            return this;
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
            if (_logFile != null)
                _logFile.WriteLine(message);
            else Console.WriteLine(message);
        }
        public void AppendLine(string message)
        {
            message = message.Replace("\0", string.Empty);
            message = message.Trim();
            message = message.Replace("\r\n", " <NL> ");
            message = DateTime.Now + "  " + message.Trim();
            if (_logFile != null)
                _logFile.WriteLine(message);
            else Console.WriteLine(message);
        }
        public void Append(string message)
        {
            if (_logFile != null)
                _logFile.Write(message);
            else Console.Write(message);
        }
    }
}
