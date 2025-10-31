using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Iit.Fibertest.Graph
{
    public static class CryptographyNew
    {
        // ⚠️ Лучше хранить ключ отдельно (например, в app.config или защищённом хранилище)
        private static readonly byte[] Key = {
        0x97, 0xdc, 0xa0, 0x54, 0x89, 0x1d, 0xe6, 0xc5,
        0x51, 0xf6, 0x4e, 0x62, 0x3f, 0x27, 0x00, 0xca
    };

        /// <summary>
        /// Шифрует объект в байты. В начало добавляется уникальный IV.
        /// </summary>
        public static byte[] Encode<T>(T obj)
        {
            if (obj == null)
                return null;

            // сериализация в JSON
            string json = JsonConvert.SerializeObject(obj);
            byte[] plainBytes = Encoding.UTF8.GetBytes(json);

            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.GenerateIV(); // уникальный IV

                using (var ms = new MemoryStream())
                {
                    // записываем IV в начало
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                    }

                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Расшифровывает байты и десериализует обратно в объект.
        /// </summary>
        public static T Decode<T>(byte[] encryptedBytes)
        {
            if (encryptedBytes == null || encryptedBytes.Length < 16)
                return default(T);

            using (var aes = Aes.Create())
            {
                aes.Key = Key;

                // читаем IV (первые 16 байт)
                byte[] iv = new byte[aes.BlockSize / 8];
                Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
                aes.IV = iv;

                // оставшиеся байты — зашифрованный контент
                byte[] cipherText = new byte[encryptedBytes.Length - iv.Length];
                Array.Copy(encryptedBytes, iv.Length, cipherText, 0, cipherText.Length);

                using (var ms = new MemoryStream(cipherText))
                using (var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var sr = new StreamReader(cryptoStream, Encoding.UTF8))
                {
                    string json = sr.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
        }
    }
}
