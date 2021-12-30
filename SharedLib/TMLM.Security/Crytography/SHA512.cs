/*********************************************************************************
 * This file is part of TMLM portal project.                                 
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
    public class TMLMPassword {
        public static string SHA512PasswordHash(string Password, string PasswordSalt) {
            SHA512Managed alg = new SHA512Managed();
            byte[] _bytHash = alg.ComputeHash(Encoding.UTF8.GetBytes(string.Format("{0}{1}", Password, PasswordSalt)));
            string _strReturn = String.Concat(Array.ConvertAll(_bytHash, x => x.ToString("X2")));
            return _strReturn;
        }
    }
}
