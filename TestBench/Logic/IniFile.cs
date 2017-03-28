using System.Runtime.InteropServices;
using System.Text;

namespace Iit.Fibertest.TestBench
{
    public class IniFile
    {
        private readonly string _filePath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, 
            string key, string def, StringBuilder retVal, int size, string filePath);

        public IniFile(string iniFilePath)
        {
            _filePath = iniFilePath;
        }

        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, _filePath);
        }

        public void Write(string section, string key, bool value)
        {
            WritePrivateProfileString(section, key, value.ToString(), _filePath);
        }

        public string Read(string section, string key, string defaultValue)
        {
            StringBuilder temp = new StringBuilder(255);
            if (GetPrivateProfileString(section, key, "", temp, 255, _filePath) != 0)
            {
                return temp.ToString();
            }

            Write(section, key, defaultValue);
            return defaultValue;
        }

        public bool Read(string section, string key, bool defaultValue)
        {
            bool result;
            return bool.TryParse(Read(section, key, defaultValue.ToString()), out result) ? result : defaultValue;
        }
    }
}
