using System.Security.Cryptography;
using System.Text;
using Dia_CoreAPI.Services.Interfaces;

namespace Dia_CoreAPI.Services.Implementations
{
    public class SecurityService : ISecurityService
    {
        public string EncryptPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
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
