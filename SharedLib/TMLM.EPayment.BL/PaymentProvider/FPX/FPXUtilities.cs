using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using TMLM.EPayment.BL.Data.EMandate;
using TMLM.EPayment.BL.Data.FPXPayment;
using TMLM.EPayment.BL.Gateway;
using TMLM.EPayment.BL.Helpers;
using TMLM.EPayment.BL.Service.Payment;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.Db.Tables;

/// <summary>
/// Summary description for Controller
///  Code       : Description
///  00         : Your signature has been verified successfully.  
///  03         : Execption Handling
///  06         : No Certificate found 
///  07         : One Certificate Found and Expired
///  08         : Both Certificates Expired
///  09         : Your Data cannot be verified against the Signature.
/// </summary>
/// 
namespace TMLM.EPayment.BL.PaymentProvider.FPX
{
    public class FPXUtilities
    {
        #region Encryption

        string ErrorCode = "XX";

        public string RSASign(string privateKeyPath, string data)
        {
            RSACryptoServiceProvider rsaCsp = LoadCertificateFile(privateKeyPath);
            byte[] dataBytes = System.Text.Encoding.Default.GetBytes(data);
            byte[] signatureBytes = rsaCsp.SignData(dataBytes, "SHA1");
            return BitConverter.ToString(signatureBytes).Replace("-", null);
        }

        byte[] GetPem(string type, byte[] data)
        {
            string pem = Encoding.UTF8.GetString(data);
            string header = String.Format("-----BEGIN {0}-----\\n", type);
            string footer = String.Format("-----END {0}-----", type);
            int start = pem.IndexOf(header) + header.Length;
            int end = pem.IndexOf(footer, start);
            string base64 = pem.Substring(start, (end - start));
            return Convert.FromBase64String(base64);
        }

        private byte[] HexToBytes(string hex)
        {
            hex = hex.Trim();

            byte[] bytes = new byte[hex.Length / 2];

            for (int index = 0; index < bytes.Length; index++)
            {
                bytes[index] = byte.Parse(hex.Substring(index * 2, 2), NumberStyles.HexNumber);
                //	Console.WriteLine("bytes: " + bytes);
            }

            return bytes;
        }

        RSACryptoServiceProvider LoadCertificateFile(string filename)
        {
            using (System.IO.FileStream fs = System.IO.File.OpenRead(filename))
            {
                byte[] data = new byte[fs.Length];
                byte[] res = null;
                fs.Read(data, 0, data.Length);
                if (data[0] != 0x30)
                {
                    res = GetPem("RSA PRIVATE KEY", data);
                }
                try
                {
                    RSACryptoServiceProvider rsa = DecodeRSAPrivateKey(res);
                    return rsa;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ex :" + ex);
                }
                return null;
            }
        }

        bool verbose = false;

        private RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            // --------- Set up stream to decode the asn.1 encoded RSA private key ------
            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem);  //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;
            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();    //advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102) //version number
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x00)
                    return null;


                //------ all private key components are Integer sequences ----
                elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);

                Console.WriteLine("showing components ..");
                if (verbose)
                {
                    showBytes("\nModulus", MODULUS);
                    showBytes("\nExponent", E);
                    showBytes("\nD", D);
                    showBytes("\nP", P);
                    showBytes("\nQ", Q);
                    showBytes("\nDP", DP);
                    showBytes("\nDQ", DQ);
                    showBytes("\nIQ", IQ);
                }

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                CspParameters CspParameters = new CspParameters();
                CspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(1024, CspParameters);
                RSAParameters RSAparams = new RSAParameters();
                RSAparams.Modulus = MODULUS;
                RSAparams.Exponent = E;
                RSAparams.D = D;
                RSAparams.P = P;
                RSAparams.Q = Q;
                RSAparams.DP = DP;
                RSAparams.DQ = DQ;
                RSAparams.InverseQ = IQ;
                RSA.ImportParameters(RSAparams);
                return RSA;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ex1 :" + ex);
                return null;
            }
            finally
            {
                binr.Close();
            }
        }

        private int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)     //expect integer
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();    // data size in next byte
            else
                if (bt == 0x82)
            {
                highbyte = binr.ReadByte(); // data size in next 2 bytes
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;     // we already have the data size
            }

            while (binr.ReadByte() == 0x00)
            {   //remove high order zeros in data
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);       //last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }

        private void showBytes(String info, byte[] data)
        {
            Console.WriteLine("{0} [{1} bytes]", info, data.Length);
            for (int i = 1; i <= data.Length; i++)
            {
                Console.Write("{0:X2} ", data[i - 1]);
                if (i % 16 == 0)
                    Console.WriteLine();
            }
            Console.WriteLine("\n\n");
        }

        public string nvl_VerifiMsg(string publicCertPath, string plainText, string encryptedstring)
        {
            string[] list = new string[2];
            list[0] = publicCertPath;
            //list[0] = (physicalApplicationPath + "fpxuat_current.cer");      //Old Certificate 
            //list[1] = (physicalApplicationPath + "fpxuat.cer");              //New Certificate       
            string returnMsg = "Error";
            try
            {
                ArrayList certs = GetCerts(list);
                RSACryptoServiceProvider rsaEncryptor;
                Boolean checkCert = false;
                byte[] plainData = System.Text.Encoding.Default.GetBytes(plainText);
                byte[] signatureData = HexToBytes(encryptedstring);
                Console.WriteLine("certs.Count : " + certs.Count);
                if (certs.Count == 1)
                {
                    rsaEncryptor = (RSACryptoServiceProvider)(((X509Certificate2)certs[0]).PublicKey.Key);
                    checkCert = rsaEncryptor.VerifyData(plainData, "SHA1", signatureData);
                }
                else if (certs.Count == 2) //Checks either any Cert should be valid on same date of expiration 
                {
                    rsaEncryptor = (RSACryptoServiceProvider)(((X509Certificate2)certs[0]).PublicKey.Key);
                    checkCert = rsaEncryptor.VerifyData(plainData, "SHA1", signatureData);
                    if (!checkCert)
                    {
                        rsaEncryptor = (RSACryptoServiceProvider)(((X509Certificate2)certs[1]).PublicKey.Key);
                        checkCert = rsaEncryptor.VerifyData(plainData, "SHA1", signatureData);
                    }

                }
                else
                {
                    returnMsg = "Invalid Certificates. " + "Code : [" + ErrorCode + "]";  //No Certificate (or) All Certificate are Expired 
                    return returnMsg;
                }

                if (checkCert)
                {
                    ErrorCode = "00";
                    returnMsg = "[" + ErrorCode + "]" + " Your signature has been verified successfully. ";
                }
                else
                {
                    ErrorCode = "09";
                    returnMsg = "[" + ErrorCode + "]" + " Your Data cannot be verified against the Signature. ";
                }

            }
            catch (Exception e)
            {
                ErrorCode = "03";
                returnMsg = "[" + ErrorCode + "] ERROR :: " + e.Message;
            }

            //return returnMsg;
            return ErrorCode;
        }

        private ArrayList GetCerts(string[] list)
        {
            int cert_exists = 0;
            ArrayList Certs = new ArrayList();
            X509Certificate2 x509_2;
            for (int i = 0; i < 2; i++)
            {
                Console.WriteLine("LOOP [" + i + "] : [" + list[i] + "]");
                if (!File.Exists(list[i]))
                {
                    cert_exists++;
                    continue;
                }
                x509_2 = new X509Certificate2(list[i]);
                String[] date = x509_2.GetExpirationDateString().Split(' ');
                //DateTime CertDate = DateTime.ParseExact(date[0], "M/dd/yyyy", null);
                if (!DateTime.TryParseExact(date[0], "M/dd/yyyy", null, DateTimeStyles.None, out DateTime CertDate))
                    CertDate = DateTime.Parse(date[0]);
                CertDate = CertDate.AddDays(-1);
                Console.WriteLine("\t" + CertDate.Date + " - " + DateTime.Today.Date);

                if (CertDate.Date > DateTime.Today.Date)
                {
                    Console.WriteLine("Passed " + i + " : " + list[i]);
                    if (i > 0)  //Roll Over the FPX.cer  Current Certificate to FPX_CURRENT.cer (IF FPX_CURRENT.cer ALREADY EXISTS AND EXPIRED WILL BE RENAMED TO FPX_CURRENT_<CURRENT DATE>.cer )
                    {
                        if (Certrollover(list[i], list[i - 1]))
                            Console.WriteLine("Roll Over Completed at Level 1");
                    }
                    Certs.Add((X509Certificate2)x509_2);
                    return Certs;
                }
                else if (CertDate.Date == DateTime.Today.Date)
                {

                    if (i > 0 && (!File.Exists(list[i - 1])))  //Roll Over the FPX.cer  Current Certificate to FPX_CURRENT.cer (IF FPX_CURRENT.cer ALREADY EXISTS AND EXPIRED WILL BE RENAMED TO FPX_CURRENT_<CURRENT DATE>.cer )
                    {
                        if (Certrollover(list[i], list[i - 1]))
                            Console.WriteLine("Roll Over Completed at Level 2");
                        Certs.Add((X509Certificate2)x509_2);
                        return Certs;
                    }


                    i++;
                    if (i < 2)
                    {
                        if (!File.Exists(list[i]))
                        {
                            Console.WriteLine("Failed to read Second Certificate  " + list[i]);
                            Certs.Add((X509Certificate2)x509_2);
                            return Certs;
                        }
                        Certs.Add(new X509Certificate2(list[i]));
                    }

                    Certs.Add((X509Certificate2)x509_2);


                    return Certs;
                }
            }
            if (cert_exists == 2)
                ErrorCode = "06";
            else if (Certs.Count == 0 && cert_exists == 1)
                ErrorCode = "07";
            else if (Certs.Count == 0 && cert_exists == 0)
                ErrorCode = "08";
            return Certs;
        }

        private bool Certrollover(string old_cert, string new_cert)
        {

            Console.WriteLine("Roll Over the Current Certificate old_cert[" + old_cert + "]   new_cert[" + new_cert + "]");
            if (File.Exists(new_cert))
            {
                String current_time_stamp = "_Old_" + (DateTime.Now).ToString("yyyyMMddHHmmssffff");
                Console.WriteLine("File.Exists : " + new_cert);
                System.IO.File.Move(new_cert, new_cert + current_time_stamp);              //FPX_CURRENT.cer to FPX_CURRENT.cer_<CURRENT TIMESTAMP>
                Console.WriteLine("Moved  " + new_cert + " to " + new_cert + current_time_stamp);
            }

            if ((!File.Exists(new_cert)) && File.Exists(old_cert))
            {
                System.IO.File.Move(old_cert, new_cert);                                    //FPX.cer to FPX_CURRENT.cer
                Console.WriteLine("Moved  " + old_cert + " to " + new_cert);
            }

            return true;
        }

        #endregion

        public static class MsgToken
        {
            public static string INDIVIDUAL = "01";
            public static string CORPORATE = "02";
        }

        /// <summary>
        /// Get active bank list from fpx
        /// </summary>
        /// <param name="msgToken">01-Individual,02-Corporate</param>
        /// <returns></returns>
        public Dictionary<string, string> GetBankList(string msgToken, string transactionNo, string paymentType = "fpx")
        {
            var result = new Dictionary<string, string>();
            var resp = string.Empty;
            FpxDecryptOutputModel FpxOutputModel = new FpxDecryptOutputModel();
            EmandateDecryptOutputModel EmandateOutputModel = new EmandateDecryptOutputModel();


           
            ApplicationAccount applicationAccountInfo = null;
            if (paymentType == "fpx")
            {
                PaymentTransaction paymentTransaction = null;
                using (var fpxrepoPayment = new PaymentTransactionRepository())
                    paymentTransaction = fpxrepoPayment.GetPaymentTransactionByTransactionNumber(transactionNo);

                using (var repoPayment = new ApplicationAccountService())
                {
                    applicationAccountInfo = repoPayment.GetApplicationAccountById(paymentTransaction.ApplicationAccountId);
                }

                FpxDecryptInputModel FpxInputModel = new FpxDecryptInputModel
                {
                    FPXSellerExchangeId = applicationAccountInfo.FPXSellerExchangeId,
                    FPXSellerId = applicationAccountInfo.FPXSellerId
                };
                FpxOutputModel = DecryptFPXHepler.FpxDecrypt(FpxInputModel);
            }
            else
            {
                EMandateTransaction eMandateTransaction = null;
                using (var emandaterepoPayment = new EMandateTransactionRepository())
                    eMandateTransaction = emandaterepoPayment.GetPaymentTransactionByTransactionNumber(transactionNo);

                using (var repoPayment = new ApplicationAccountService())
                {
                    applicationAccountInfo = repoPayment.GetApplicationAccountById(eMandateTransaction.ApplicationAccountId);
                }


                EmandateDecryptInputModel EmandateInputModel = new EmandateDecryptInputModel
                {
                    EmandateSellerExchangeId = applicationAccountInfo.EMandateSellerExchangeId,
                    EmandateSellerId = applicationAccountInfo.EMandateSellerId
                };


                EmandateOutputModel = DecryptFPXHepler.EmandateDecrypt(EmandateInputModel);
            }
            //get active bank list from api
            try
            {
                var checkSum = "";
                var postingData = "";

                var msgType = "BE";
                if (paymentType == "fpx")
                {
                    checkSum = msgToken + "|" + msgType + "|" + FpxOutputModel.FPXSellerExchangeId + "|"
                        + applicationAccountInfo.FPXVersion;
                    checkSum = checkSum.Trim();

                    checkSum = RSASign(applicationAccountInfo.FPXPrivateKeyPath, checkSum);

                    postingData = "fpxmsgToken=" + msgToken + "&fpx_msgToken=" + msgToken + "&fpx_msgType=" + msgType + "&fpx_sellerExId=" +
                        FpxOutputModel.FPXSellerExchangeId + "&fpx_version=" + applicationAccountInfo.FPXVersion + "&fpx_checkSum=" +
                        checkSum;
                }
                else
                {
                    checkSum = msgToken + "|" + msgType + "|" + EmandateOutputModel.EmandateSellerExchangeId + "|"
                        + applicationAccountInfo.EMandateFPX_Version;
                    checkSum = checkSum.Trim();

                    checkSum = RSASign(applicationAccountInfo.EMandatePrivateKeyPath, checkSum);

                    postingData = "fpxmsgToken=" + msgToken + "&fpx_msgToken=" + msgToken + "&fpx_msgType=" + msgType + "&fpx_sellerExId=" +
                        EmandateOutputModel.EmandateSellerExchangeId + "&fpx_version=" + applicationAccountInfo.EMandateFPX_Version + "&fpx_checkSum=" +
                        checkSum;
                }

                LogManager.GetLogger(this.GetType()).Info("BE message : " + postingData);

                var apiRequest = new ApiRequest();
                apiRequest.Payload = postingData;
                apiRequest.IgnoreSslErrors = true;
                apiRequest.RequestUrl = FPXSettings.BASE_URL + "FPXMain/RetrieveBankList";
                apiRequest.ApiMethod = ApiClient.POST;
                apiRequest.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                apiRequest.Accept = "application/json";
                apiRequest.AllowAutoRedirect = true;
                apiRequest.KeepAlive = false;
                apiRequest.Timeout = 300000;

                var apiClient = new ApiClient();
                resp = apiClient.SendTransaction(apiRequest);
                LogManager.GetLogger(this.GetType()).Info("BE message : " + apiRequest);
            }
            catch (Exception exception)
            {
                return result;
            }

            if (resp.Equals("ERROR"))
                return result;

            var qscoll = System.Web.HttpUtility.ParseQueryString(resp);

            LogManager.GetLogger(this.GetType()).Info("BC message : " + resp);
            LogManager.GetLogger(this.GetType()).Info("BC message : " + qscoll);

            //validate checksum
            var checkSumString = qscoll["fpx_bankList"] + "|" + qscoll["fpx_msgToken"] + "|" + qscoll["fpx_msgType"] + "|" + qscoll["fpx_sellerExId"];
            if (nvl_VerifiMsg(applicationAccountInfo.FPXPublicCertPath, checkSumString, qscoll["fpx_checkSum"]) != "00")
                return result;

            var bankResponse = (qscoll["fpx_bankList"].Replace("%7E", "~")).Replace("%2C", ", ");

            var apiBanks = bankResponse.Split(',');

            //get bank list from db
            var dbBanks = new List<BankList>();
            using (var _repoBankList = new BankListRepository())
                dbBanks = _repoBankList.GetAllBankList();

            foreach (var bank in apiBanks.Where(x => !string.IsNullOrEmpty(x)))
            {
                var bankCode = bank.Split('~');
                var offlineText = string.Empty;
                if (bankCode[1] == "B")
                    offlineText = " (Offline)";

                var bankName = dbBanks.FirstOrDefault(x => x.BankCode == bankCode[0] && x.MsgToken == msgToken);
                if (bankName != null)
                    result.Add(bankCode[0], bankName.BankName + offlineText);
            }

            return result;
        }

        public string GetOrdernum(string orderno, string paymentRef, int mode)
        {
            if (mode == 0 && !string.IsNullOrEmpty(paymentRef))
            {
                return paymentRef;
            }
            return orderno;
        }

        public string GetOrdernum(string orderNo, string paymentRef, string orderNoFpx, int mode)
        {
            if (mode == 0 && !string.IsNullOrEmpty(paymentRef))
            {
                return orderNo;
            }
            return orderNoFpx;
        }
    }
}
