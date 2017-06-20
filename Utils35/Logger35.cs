using System;
using System.Globalization;
using System.IO;

namespace Iit.Fibertest.Utils35
{
    public class Logger35
    {
        private StreamWriter _logFile;
        private string _culture;

        private readonly object _obj = new object();

        public void AssignFile(string filename, string culture ="ru-RU")
        {
            if (filename == "")
                return;

            lock (_obj)
            {
                var logFullFileName = Utils.FileNameForSure(@"..\Log\", filename, true);
                if (logFullFileName == null)
                    return;
                _logFile = File.AppendText(logFullFileName);
                _logFile.AutoFlush = true;

                _culture = culture;
            }
        }

        public void EmptyLine(char ch = ' ')
        {
            lock (_obj)
            {
                string message = new string(ch, 78);
                if (_logFile != null)
                    _logFile.WriteLine(message);
                else Console.WriteLine(message);
            }
        }

        public void AppendLine(string message, int offset = 0, string prefix = "")
        {
            lock (_obj)
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
                    var msg = DateTime.Now.ToString(new CultureInfo(_culture)) + "  " + offsetStr + prefix + str.Trim();
                    if (_logFile != null)
                        _logFile.WriteLine(msg);
                    else Console.WriteLine(msg);
                }
            }
        }

    }
}
