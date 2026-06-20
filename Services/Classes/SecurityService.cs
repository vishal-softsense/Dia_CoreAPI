using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Dia_CoreAPI.Services.Interfaces;

namespace Dia_CoreAPI.Services.Classes
{
    public class SecurityService : ISecurityService
    {
        public string EncryptPassword(string input)
        {
            string password = "Z2rp2Y^985C2^4A%Z";
            try
            {
                byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                using (var sha256 = SHA256.Create())
                {
                    passwordBytes = sha256.ComputeHash(passwordBytes);
                }

                byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);
                string result = Convert.ToBase64String(bytesEncrypted);

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
#pragma warning disable SYSLIB0022 // Rijndael types are obsolete
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

#pragma warning disable SYSLIB0041 // Rfc2898DeriveBytes constructor is obsolete
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
#pragma warning restore SYSLIB0041
                    
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
#pragma warning restore SYSLIB0022
            }

            return encryptedBytes;
        }
    }
}
