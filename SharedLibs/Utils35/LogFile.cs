using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Iit.Fibertest.UtilsLib
{
    public class LogFile
    {
        private const string ToCompress = ".toCompress";

        private StreamWriter _logFile;
        private string _culture;
        private int _sizeLimitKb;
        private string _logFullFileName;

        private readonly object _obj = new object();

        public void AssignFile(string filename, int sizeLimitKb, string culture = "ru-RU")
        {
            if (filename == "")
                return;

            lock (_obj)
            {
                _logFullFileName = Utils.FileNameForSure(@"..\Log\", filename, true);
                if (_logFullFileName == null)
                    return;
                _logFile = File.AppendText(_logFullFileName);
                _logFile.AutoFlush = true;

                _sizeLimitKb = sizeLimitKb;
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
                if (_logFile == null)
                    return;

                CheckSizeLimit();

                var offsetStr = new string(' ', offset);
                if (!string.IsNullOrEmpty(prefix))
                    prefix += " ";
                foreach (var str in SplitMessageOnLines(message))
                {
                    var msg = DateTime.Now.ToString(new CultureInfo(_culture)) + "  " + offsetStr + prefix + str.Trim();
                    _logFile.WriteLine(msg);
                }
            }
        }

        private static string[] SplitMessageOnLines(string message)
        {
            message = message.Replace("\0", string.Empty);
            message = message.Trim();
            message = message.Replace("\r\n", "\r");
            message = message.Replace("\n\r", "\r");
            message = message.Replace("\n", "\r");
            return message.Split('\r');
        }

        private void CheckSizeLimit()
        {
            if (_sizeLimitKb > 0 && _logFile.BaseStream.Length > _sizeLimitKb * 1024)
            {
                _logFile.Close();
                File.Copy(_logFullFileName, _logFullFileName + ToCompress, true);
                var newEmptyLogFile = Utils.FileNameForSure(@"..\Log\", "empty.log", true);
                File.Copy(newEmptyLogFile, _logFullFileName, true);

                _logFile = File.AppendText(_logFullFileName);
                _logFile.AutoFlush = true;
                File.Delete(Path.Combine(Path.GetDirectoryName(_logFullFileName), @"empty.log"));

                var thread = new Thread(Pack);
                thread.Start();
            }
        }

        private void Pack()
        {
            FileInfo fileToCompress = new FileInfo(_logFullFileName + ToCompress);
            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                FileStream compressedFileStream;
                try
                {
                    compressedFileStream = File.Create(_logFullFileName + ".gz");
                }
                catch (Exception)
                {
                    compressedFileStream = File.Create(_logFullFileName + $".{Guid.NewGuid()}.gz");
                }

                using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                {
                    originalFileStream.CopyTo(compressionStream);
                }
            }
            File.Delete(_logFullFileName + ToCompress);
        }
    }
}
