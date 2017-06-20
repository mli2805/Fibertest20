using System.Runtime.InteropServices;
using System.Text;

namespace Iit.Fibertest.Utils35
{
    public class IniFile
    {
        private string _filePath;
        private readonly object _obj = new object();

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
            string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// should be done separately from creation for tests sake
        /// </summary>
        /// <param name="fullFilename"></param>
        public void AssignFile(string fullFilename)
        {
            lock (_obj)
            {
                _filePath = Utils.FileNameForSure(@"..\Ini\", fullFilename, false);
            }
        }

        #region Base (String)

        public void Write(IniSection section, IniKey key, string value)
        {
            lock (_obj)
            {
                WritePrivateProfileString(section.ToString(), key.ToString(), value, _filePath);
            }
        }

        public string Read(IniSection section, IniKey key, string defaultValue)
        {
            lock (_obj)
            {
                StringBuilder temp = new StringBuilder(255);
                if (GetPrivateProfileString(section.ToString(), key.ToString(), "", temp, 255, _filePath) != 0)
                {
                    return temp.ToString();
                }

                Write(section, key, defaultValue);
                return defaultValue;
            }
        }

        #endregion

        #region Extensions (Other classes)

        public void Write(IniSection section, IniKey key, bool value)
        {
            Write(section, key, value.ToString());
        }

        public void Write(IniSection section, IniKey key, int value)
        {
            Write(section, key, value.ToString());
        }

        public bool Read(IniSection section, IniKey key, bool defaultValue)
        {
            bool result;
            return bool.TryParse(Read(section, key, defaultValue.ToString()), out result) ? result : defaultValue;
        }

        public int Read(IniSection section, IniKey key, int defaultValue)
        {
            int result;
            return int.TryParse(Read(section, key, defaultValue.ToString()), out result) ? result : defaultValue;
        }

        #endregion
    }
}