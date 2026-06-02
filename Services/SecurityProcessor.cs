using System.Security.Cryptography;
using System.Text;

namespace Dia_CoreAPI.Services
{
    public class SecurityProcessor
    {
        public static string EncryptPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes =
                    sha.ComputeHash(
                        Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();

                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
