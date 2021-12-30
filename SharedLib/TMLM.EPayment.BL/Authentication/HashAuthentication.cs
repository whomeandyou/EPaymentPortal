
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Web;
using System.Net.Http;
using System.IO;
using System.Transactions;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

using TMLM;
using TMLM.Common;
using System.Security.Cryptography;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.BL.Service.Payment;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.BL.Service.Authentication
{
    public class HashAuthentication : IDisposable
    {
        #region Utilities

        private static string ComputeHash(HashAlgorithm hash, string plainText)
        {
            var UE = new UTF8Encoding();
            byte[] HashValue = null;
            byte[] MessageBytes = UE.GetBytes(plainText);
            string strHex = "";

            HashValue = hash.ComputeHash(MessageBytes);
            foreach (byte b in HashValue)
                strHex += String.Format("{0:x2}", b);

            return strHex;
        }

        #endregion

        // Flag: Has Dispose already been called?
        bool disposed = false;

        public HashAuthentication() { }

        public bool CompareRequestSignature(string amount, string orderNumber,
            string applicationAccountCode, string msgSign)
        {
            var secretKey = string.Empty;
            using (var repoApplicationAccount = new ApplicationAccountService())
            {
                var applicationAccount = repoApplicationAccount.GetApplicationAccountByCode(applicationAccountCode);
                if (applicationAccount == null)
                    return false;

                secretKey = applicationAccount.SecretKey;
            }

            var signedData = "POST" + //http-method is hardcoded to post only
                amount + orderNumber + applicationAccountCode + secretKey;

            return msgSign == ComputeHash(new SHA256Managed(), signedData);
        }

        public string GetResponseSignature(string amount, string orderNumber, int transactionStatusId,
            string applicationAccountCode, string CCCardNumber)
        {
            var secretKey = string.Empty;
            using (var repoApplicationAccount = new ApplicationAccountService())
            {
                ApplicationAccount applicationAccount = null;
                applicationAccount = repoApplicationAccount.GetApplicationAccountByCode(applicationAccountCode);
                if (applicationAccount == null)
                {
                    var isNumeric = int.TryParse(applicationAccountCode, out int applicationID);
                    if (isNumeric)
                    {
                        applicationAccount = repoApplicationAccount.GetApplicationAccountById(applicationID);
                        if (applicationAccount == null)
                            return string.Empty;
                    }
                }

                secretKey = applicationAccount.SecretKey;
            }

            var signedData = "POST" + //http-method is hardcoded to post only
                amount + orderNumber + transactionStatusId + applicationAccountCode + CCCardNumber + secretKey;

            return ComputeHash(new SHA256Managed(), signedData);
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
            }
            disposed = true;
        }
    }
}
