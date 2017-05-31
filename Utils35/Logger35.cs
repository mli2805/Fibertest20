using System;
using System.IO;

namespace Iit.Fibertest.Utils35
{
    public class Logger35
    {
        private StreamWriter _logFile;

        public void AssignFile(string filename)
        {
            if (filename != "")
            {
                var logFullFileName = Utils.FileNameForSure(@"..\Log\", filename, true);
                if (logFullFileName == null)
                    return;
                _logFile = File.AppendText(logFullFileName);
                _logFile.AutoFlush = true;
            }
        }

        public void EmptyLine(char ch = ' ')
        {
            string message = new string(ch, 78);
            if (_logFile != null)
                _logFile.WriteLine(message);
            else Console.WriteLine(message);
        }

        public void AppendLine(string message, int offset = 0, string prefix = "")
        {
            message = message.Replace("\0", string.Empty);
            message = message.Trim();
            message = message.Replace("\r\n", "\r");
            message = message.Replace("\n\r", "\r");
            message = message.Replace("\n", "\r");
            var content = message.Split('\r');

            var offsetStr = new string(' ', offset);
            if (!string.IsNullOrEmpty(prefix))
                prefix += " ";
            foreach (var str in content)
            {
                var msg = DateTime.Now + "  " + offsetStr + prefix + str.Trim();
                if (_logFile != null)
                    _logFile.WriteLine(msg);
                else Console.WriteLine(msg);
            }
        }

        public void Append(string message, int offset = 0, string prefix = "")
        {
            var offsetStr = new string(' ', offset);
            if (!string.IsNullOrEmpty(prefix))
                prefix += " ";
            var msg = DateTime.Now + "  " + offsetStr + prefix + message.Trim();
            if (_logFile != null)
                _logFile.Write(msg);
            else Console.Write(msg);
        }

        public void AddOnTheSameString(string message)
        {
            if (_logFile != null)
                _logFile.WriteLine(message);
            else Console.WriteLine(message);
        }
    }
}
