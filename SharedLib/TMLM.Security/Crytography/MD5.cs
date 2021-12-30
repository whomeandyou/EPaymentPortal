/*********************************************************************************
 *       
 *                                                                               
 * This file is part of TMLM Portal project.                                 
 * Unauthorized copying of this file or any of the part is strictly prohibited.  
 * Proprietary and confidential                                                  
 *                                                                               
 * Written by Teong Wah, Feb 2019                
 *                                                                               
 *********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace TMLM.Security.Crytography {
    public class MD5Util {
        public static string CalculateMD5Hash(string input) {

            // step 1, calculate MD5 hash from input

            MD5 md5 = System.Security.Cryptography.MD5.Create();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++) {

                sb.Append(hash[i].ToString("X2"));

            }

            return sb.ToString();

        }
    }
}
