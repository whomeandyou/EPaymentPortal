using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TMLM.Common
{
    public class Utils
    {
        //
        public static string RandomString(int Length)
        {
            System.Random random = new System.Random();
            string str = string.Empty;
            for (int i = 0; i < Length; i++)
            {
                str = str + new string((char)random.Next(0x21, 0x7e), 1);
            }
            return str;
        }

        /// <summary>
        /// Use to generate random string in the length provided, 
        /// </summary>
        /// <param name="AlphaNumeric">Non symbolic</param>
        /// <param name="Length">Length of String need</param>
        /// <returns>string</returns>
        public static string RandomString(int Length, bool AlphaNumeric)
        {
            System.Random _rnd = new System.Random();
            string _strReturn = string.Empty;
            int i = 0;
            if (AlphaNumeric)
            {
                while (i < Length)
                {
                    int _iRndNumber = _rnd.Next(33, 122);

                    if ((_iRndNumber >= 48 && _iRndNumber <= 57) || (_iRndNumber >= 65 && _iRndNumber <= 90) ||
                        (_iRndNumber >= 97 && _iRndNumber <= 122))
                    {
                        _strReturn += new string((char)_iRndNumber, 1);
                        i++;
                    }
                }
                return _strReturn;
            }
            else
                return Utils.RandomString(Length);
        }

        public static DataTable ToDataTable<T>(IEnumerable<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static string decryptCCNumber(string ccNumber)
        {
            string[] splitCardString = null;
            if (!string.IsNullOrEmpty(ccNumber))
            {
                splitCardString = ccNumber.Split('|');
            }
            else
            {
                return string.Empty;
            }

            string cardNumberString = string.Empty;
            if (splitCardString.Count() > 1 && !string.IsNullOrEmpty(splitCardString[1].ToString()))
            {

                cardNumberString = DecryptStringCreditCard(splitCardString[0].ToString(), splitCardString[1].ToString());
            }

            return cardNumberString;
        }

        private static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private static string DecryptStringCreditCard(string cipherText, string ivkey)
        {
            // Create key
            string AesSecretKey = "TMlM@534D-4711-B353-2F6A815BA490";
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(AesSecretKey));
            //byte[] key = Encoding.ASCII.GetBytes(AesSecretKey);

            // Create secret IV
            byte[] iv = StringToByteArray(ivkey);
            //byte[] iv = Encoding.ASCII.GetBytes(ivkey); //new byte[16] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;

            // Set key and IV
            byte[] aesKey = new byte[32];
            Array.Copy(key, 0, aesKey, 0, 32);
            encryptor.Key = aesKey;
            encryptor.IV = iv;

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our Aes object
            ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);

            // Will contain decrypted plaintext
            string plainText = String.Empty;

            try
            {
                // Convert the ciphertext string into a byte array
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                // Decrypt the input ciphertext string
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                // Complete the decryption process
                cryptoStream.FlushFinalBlock();

                // Convert the decrypted data from a MemoryStream to a byte array
                byte[] plainBytes = memoryStream.ToArray();

                // Convert the decrypted byte array to string
                plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                // Close both the MemoryStream and the CryptoStream
                memoryStream.Close();
                cryptoStream.Close();
            }

            // Return the decrypted data as a string
            return plainText;
        }       
    }
}
