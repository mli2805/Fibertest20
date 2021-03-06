﻿using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.UtilsLib
{
    public class IniFile
    {
        public string FilePath { get; private set; }
        private readonly object _obj = new object();

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
            string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// should be done separately from creation for tests sake
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="isFileReady"></param>
        public IniFile AssignFile(string filename, bool isFileReady = false)
        {
            lock (_obj)
            {
                FilePath = isFileReady ? filename : Utils.FileNameForSure(@"..\Ini\", filename, false);
            }
            return this;
        }

        #region Base (String)

        public void Write(IniSection section, IniKey key, string value)
        {
            lock (_obj)
            {
                WritePrivateProfileString(section.ToString(), key.ToString(), value, FilePath);
            }
        }

        public string Read(IniSection section, IniKey key, string defaultValue)
        {
            lock (_obj)
            {
                StringBuilder temp = new StringBuilder(255);
                if (GetPrivateProfileString(section.ToString(), key.ToString(), "", temp, 255, FilePath) != 0)
                {
                    return temp.ToString();
                }

                Write(section, key, defaultValue);
                return defaultValue;
            }
        }

        public string ReadForeignIni(string filepath, IniSection section, IniKey key)
        {
            StringBuilder temp = new StringBuilder(255);
            if (GetPrivateProfileString(section.ToString(), key.ToString(), "", temp, 255, filepath) != 0)
            {
                return temp.ToString();
            }
            return null;
        }

        public void DeleteKey(IniSection section, IniKey key)
        {
            lock (_obj)
            {
                WritePrivateProfileString(section.ToString(), key.ToString(), null, FilePath);
            }
        }

        #endregion

        public void DeleteSection(IniSection section)
        {
            lock (_obj)
            {
                WritePrivateProfileString(section.ToString(), null, null, FilePath);
            }
        }

        #region Extensions (Other types)

        public void Write(IniSection section, IniKey key, bool value)
        {
            Write(section, key, value.ToString());
        }

        public void Write(IniSection section, IniKey key, int value)
        {
            Write(section, key, value.ToString());
        }

        public void Write(IniSection section, IniKey key, double value)
        {
            Write(section, key, value.ToString(CultureInfo.InvariantCulture));
        }

        public bool Read(IniSection section, IniKey key, bool defaultValue)
        {
            return bool.TryParse(Read(section, key, defaultValue.ToString()), out var result) ? result : defaultValue;
        }

        public int Read(IniSection section, IniKey key, int defaultValue)
        {
            return int.TryParse(Read(section, key, defaultValue.ToString()), out var result) ? result : defaultValue;
        }

        public double Read(IniSection section, IniKey key, double defaultValue)
        {
            var str = Read(section, key, defaultValue.ToString(CultureInfo.InvariantCulture));
            return double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
        }

        public NetAddress Read(IniSection section, int useTcpPort)
        {
            var netAddress = new NetAddress
            {
                IsAddressSetAsIp = Read(section, IniKey.IsAddressIp, true),
                Port = useTcpPort,
            };
            netAddress.Ip4Address = Read(section, IniKey.Ip, netAddress.IsAddressSetAsIp ? "0.0.0.0" : "");
            netAddress.HostName = Read(section, IniKey.Host, netAddress.IsAddressSetAsIp ? "" : "localhost");

            return netAddress;
        }

        public void Write(NetAddress netAddress, IniSection section)
        {
            Write(section, IniKey.IsAddressIp, netAddress.IsAddressSetAsIp);
            Write(section, IniKey.Ip, netAddress.IsAddressSetAsIp ? netAddress.Ip4Address : "");
            Write(section, IniKey.Host, netAddress.IsAddressSetAsIp ? "" : netAddress.HostName);
        }

        public DoubleAddress ReadDoubleAddress(int defaultTcpPort)
        {
            var addresses = new DoubleAddress
            {
                Main = Read(IniSection.ServerMainAddress, defaultTcpPort),
                HasReserveAddress = Read(IniSection.Server, IniKey.HasReserveAddress, false),
            };
            if (addresses.HasReserveAddress)
                addresses.Reserve = Read(IniSection.ServerReserveAddress, defaultTcpPort);
            return addresses;
        }

        public void WriteServerAddresses(DoubleAddress doubleAddress)
        {
            Write(doubleAddress.Main, IniSection.ServerMainAddress);
            Write(IniSection.Server, IniKey.HasReserveAddress, doubleAddress.HasReserveAddress);
            if (doubleAddress.HasReserveAddress)
                Write(doubleAddress.Reserve, IniSection.ServerReserveAddress);
            else
                DeleteSection(IniSection.ServerReserveAddress);
        }
        #endregion
    }
}