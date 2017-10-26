using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Mite.Helpers
{
    public static class CryptoHelper
    {
        public static string CreateKey(string data)
        {
            var sb = new StringBuilder();
            using (var hash = SHA256.Create())
            {
                var result = hash.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(result);
            }
        }
        public async static Task<(string iv, string data)> EncryptAsync(string data, string key)
        {
            (string, string) tuple;
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Key = Convert.FromBase64String(key);
                var encryptor = aes.CreateEncryptor();
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            await sw.WriteAsync(data);
                        }
                        tuple = (Convert.ToBase64String(aes.IV), Convert.ToBase64String(ms.ToArray()));
                    }
                }
            }

            return tuple;
        }
        public async static Task<string> DecryptAsync(string encryptedData, string key, string iv)
        {
            string plainText;

            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Key = Convert.FromBase64String(key);
                aes.IV = Convert.FromBase64String(iv);
                var decryptor = aes.CreateDecryptor();
                using (var ms = new MemoryStream(Convert.FromBase64String(encryptedData)))
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (var sw = new StreamReader(cs))
                        {
                            plainText = await sw.ReadToEndAsync();
                        }
                    }
                }
            }
            return plainText;
        }
        public static string Decrypt(string encryptedData, string key, string iv)
        {
            string plainText;

            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Key = Convert.FromBase64String(key);
                aes.IV = Convert.FromBase64String(iv);
                var decryptor = aes.CreateDecryptor();
                using (var ms = new MemoryStream(Convert.FromBase64String(encryptedData)))
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (var sw = new StreamReader(cs))
                        {
                            plainText = sw.ReadToEnd();
                        }
                    }
                }
            }
            return plainText;
        }
    }
}