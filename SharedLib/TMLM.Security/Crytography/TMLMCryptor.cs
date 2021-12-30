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
    public class TMLMCryptor : IDisposable {
        // Flag: Has Dispose already been called?
        bool disposed = false;
        private RijndaelManaged _objAES = null;
        public static string IV;
        public static string KEY;

        public TMLMCryptor(byte[] iv, byte[] key) {
            this._objAES = new RijndaelManaged();
            this._objAES.BlockSize = 128;
            this._objAES.KeySize = 128;
            this._objAES.IV = iv;
            this._objAES.Key = key;
            this._objAES.Padding = PaddingMode.PKCS7;
            this._objAES.Mode = CipherMode.ECB;
        }

        public TMLMCryptor() : this(Convert.FromBase64String(TMLMCryptor.IV), Convert.FromBase64String(TMLMCryptor.KEY)) {
            
        }

        public String encrypt2String(String message, bool toBase64) {
            byte[] cipherText = this.encrypt2Byte(message);
            if (toBase64) {
                return Convert.ToBase64String(cipherText);
            } else {
                return UTF8Encoding.UTF8.GetString(cipherText);
            }
        }

        public String decrypt2String(String message, bool fromBase64) {
            byte[] plainBytes = this.decrypt2Byte(message, fromBase64);
            return Encoding.UTF8.GetString(plainBytes);
        }

        public byte[] encrypt2Byte(String message) {
            ICryptoTransform transform = _objAES.CreateEncryptor();
            byte[] _bytInput = UTF8Encoding.UTF8.GetBytes(message);
            return transform.TransformFinalBlock(_bytInput, 0, _bytInput.Length);
        }

        public byte[] decrypt2Byte(String message, bool fromBase64) {
            ICryptoTransform transform = _objAES.CreateDecryptor();
            byte[] _bytInput = null;
            if (fromBase64) {
                _bytInput = Convert.FromBase64String(message);
            } else {
                _bytInput = Encoding.UTF8.GetBytes(message);
            }
            return transform.TransformFinalBlock(_bytInput, 0, _bytInput.Length);
        }

        public static String encrypt2Base64String(String message) {
            using (TMLMCryptor _cryptor = new TMLMCryptor()) {
                return _cryptor.encrypt2String(message, true);
            }
        }

        public static String decryptBase642String(String message) {
            using (TMLMCryptor _cryptor = new TMLMCryptor()) {
                return _cryptor.decrypt2String(message, true);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            // Free any unmanaged objects here.
            if (disposing)
            {
                //stuff to dispose
                this._objAES.Dispose();
                this._objAES = null;
            }
            disposed = true;
        }
    }
}
