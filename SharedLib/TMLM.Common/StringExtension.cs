using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.Common
{
    public static class StringExtension {
        public static string ComputeHashUTF8(this string Key) {
            SHA1CryptoServiceProvider objSHA1 = new SHA1CryptoServiceProvider();
            objSHA1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Key));

            byte[] buffer = objSHA1.Hash;
            string HashValue = System.Convert.ToBase64String(buffer);
            return HashValue;
        }
    }
}
