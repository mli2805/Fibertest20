using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace Iit.Fibertest.Graph
{
    public class LicenseManager
    {
        private readonly byte[] _key = { 0x97, 0xdc, 0xa0, 0x54, 0x89, 0x1d, 0xe6, 0xc5, 0x51, 0xf6, 0x4e, 0x62, 0x3f, 0x27, 0x00, 0xca };
        private readonly byte[] _initVector = { 0xf3, 0x5e, 0x7a, 0x81, 0xae, 0xf2, 0xe7, 0xc1, 0x8d, 0x54, 0x00, 0x8c, 0xb4, 0x92, 0xd0, 0xd8 };

        public License Decode(byte[] bytes)
        {
            using (MemoryStream fStream = new MemoryStream(bytes))
            {
                var rmCrypto = new RijndaelManaged();

                using (var cryptoStream = new CryptoStream(fStream, rmCrypto.CreateDecryptor(_key, _initVector), CryptoStreamMode.Read))
                {
                    var binaryFormatter = new BinaryFormatter();
                    return (License)binaryFormatter.Deserialize(cryptoStream);
                }
            }
        }

        public byte[] Encode(License license)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    var rmCrypto = new RijndaelManaged();

                    using (var cryptoStream = new CryptoStream(memoryStream, rmCrypto.CreateEncryptor(_key, _initVector), CryptoStreamMode.Write))
                    {
                        var binaryFormatter = new BinaryFormatter();
                        binaryFormatter.Serialize(cryptoStream, license);
                    }

                    return memoryStream.ToArray();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public License ReadLicenseFromFile(string initialDirectory = "")
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = @".lic";
            dlg.InitialDirectory = initialDirectory;
            dlg.Filter = @"License file  |*.lic";
            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;
                var encoded = File.ReadAllBytes(filename);
                return Decode(encoded);
            }
            return null;
        }

    }
}
