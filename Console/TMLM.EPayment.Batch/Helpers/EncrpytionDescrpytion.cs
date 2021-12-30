using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch.Helpers
{
    public static class EncrpytionDescrpytion
    {
        public static string EncryptString(string value)
        {
            string result = string.Empty;
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(value));
                var builder = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    builder.Append(b.ToString("X2"));
                }
                result = builder.ToString();
            }
            result = result.Replace(" ", "");

            return result;
        }

        public static string DecryptString(string value)
        {
            string result = "";

            return result;
        }
    }
}
