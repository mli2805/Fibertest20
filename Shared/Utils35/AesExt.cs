using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Iit.Fibertest.UtilsLib
{
     public static class AesExt
    {
        private static readonly byte[] Key = Encoding.ASCII.GetBytes(":r5\u0002F#a-\u00172f@\u0011al8");

        public static string Encript(string str)
        {
            try
            {
                using MemoryStream myStream = new MemoryStream();

                //Create a new instance of the default Aes implementation class  
                // and configure encryption key.  
                using Aes aes = Aes.Create();
                aes.Key = Key;

                //Stores IV at the beginning of the file.
                //This information will be used for decryption.
                byte[] iv = aes.IV;
                myStream.Write(iv, 0, iv.Length);

                //Create a CryptoStream, pass it the FileStream, and encrypt
                //it with the Aes class.  
                using CryptoStream cryptStream = new CryptoStream(
                    myStream,
                    aes.CreateEncryptor(),
                    CryptoStreamMode.Write);

                //Create a StreamWriter for easy writing to the
                //file stream.  
                using StreamWriter sWriter = new StreamWriter(cryptStream);

                //Write to the stream.  
                sWriter.Write(str);
                sWriter.Close();

                return ByteArrayToString(myStream.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static string Decrypt(string str)
        {
            try
            {
                var buffer = StringToByteArray(str);
                using MemoryStream myStream = new MemoryStream(buffer);

                //Create a new instance of the default Aes implementation class
                using Aes aes = Aes.Create();

                //Reads IV value from beginning of the file.
                byte[] iv = new byte[aes.IV.Length];
                myStream.Read(iv, 0, iv.Length);

                //Create a CryptoStream, pass it the file stream, and decrypt
                //it with the Aes class using the key and IV.
                using CryptoStream cryptStream = new CryptoStream(
                    myStream,
                    aes.CreateDecryptor(Key, iv),
                    CryptoStreamMode.Read);

                //Read the stream.
                using StreamReader sReader = new StreamReader(cryptStream);
                var result = sReader.ReadToEnd();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private static byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }


}
