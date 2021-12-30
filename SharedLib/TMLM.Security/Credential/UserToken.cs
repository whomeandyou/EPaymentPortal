/*********************************************************************************
 *       
 *                                                                               
 * This file is part of TMLM Portal project.                                 
 * Unauthorized copying of this file or any of the part is strictly prohibited.  
 * Proprietary and confidential                                                  
 *                                                                               
 * Written by Teong Wah                   
 *                                                                               
 *********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using TMLM.Security.Crytography;

namespace TMLM.Security.Credential {
    public class UserToken {
        public string UserName { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Language { get; set; }
        public string DisplayName { get; set; }
        public string Branch_Name { get; set; }

        public string ToTokenValue() {
            string _strReturn = String.Format("{0}|{1}|{2}|{3}|{4}", this.ExpiryDate.ToString(), 
                this.UserName, this.DisplayName, this.Language, this.Branch_Name);
            
            //TODO: implement encryption here
            _strReturn = TMLMCryptor.encrypt2Base64String(_strReturn);

            return _strReturn;
        }

        public static UserToken FromTokenValue(string TokenValue) {
            //TODO: implement decryption here
            TokenValue = TMLMCryptor.decryptBase642String(TokenValue);

            string[] strArray2 = TokenValue.Split(new char[] { '|' });
            if (strArray2.Length != 5)
                return null;

            return new UserToken() {
                ExpiryDate = DateTime.Parse(strArray2[0]),
                UserName = strArray2[1],
                DisplayName = strArray2[2],
                Language = strArray2[3],
                Branch_Name = strArray2[4]
            };
        }
    }
}