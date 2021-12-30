using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TMLM.EPayment.BL.Data.EMandate;
using TMLM.EPayment.BL.Data.FPXPayment;
using TMLM.EPayment.BL.Data.RazerPay;


namespace TMLM.EPayment.BL.Helpers
{
    public class DecryptFPXHepler
    {
         public static FpxDecryptOutputModel FpxDecrypt(FpxDecryptInputModel inputModel)
        {
            FpxDecryptOutputModel outputModel = new FpxDecryptOutputModel();
            using (TMLM.Security.Crytography.RSA oRSA = new Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert
                = new System.Security.Cryptography.X509Certificates.X509Certificate2
                    (HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                outputModel.FPXSellerExchangeId = oRSA.Decrypt(privateKey,
                 inputModel.FPXSellerExchangeId);
                CAcert = null;
            }

            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert
                = new System.Security.Cryptography.X509Certificates.X509Certificate2
                    (HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                outputModel.FPXSellerId = oRSA.Decrypt(privateKey,
                  inputModel.FPXSellerId);
                CAcert = null;
            }

            return outputModel;
        }

        public static EmandateDecryptOutputModel EmandateDecrypt(EmandateDecryptInputModel inputmodel)
        {
            EmandateDecryptOutputModel outputModel = new EmandateDecryptOutputModel();
            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert
                = new System.Security.Cryptography.X509Certificates.X509Certificate2
                    (HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                outputModel.EmandateSellerId = oRSA.Decrypt(privateKey,
                  inputmodel.EmandateSellerId);

            }

            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert
                = new System.Security.Cryptography.X509Certificates.X509Certificate2
                    (HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                outputModel.EmandateSellerExchangeId = oRSA.Decrypt(privateKey,
                  inputmodel.EmandateSellerExchangeId);

            }

            return outputModel;
        }

        public static RazerPayDecryptOutputModel RazerPayDecrypt(string publicKey, string secretKey, string merchantId)
        {
            RazerPayDecryptOutputModel outputModel = new RazerPayDecryptOutputModel();
            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert =
                    new System.Security.Cryptography.X509Certificates.X509Certificate2(HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                if (!string.IsNullOrEmpty(publicKey))
                    outputModel.PublicKey = oRSA.Decrypt(privateKey, publicKey);
            }

            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert =
                    new System.Security.Cryptography.X509Certificates.X509Certificate2(HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                if (!string.IsNullOrEmpty(secretKey))
                    outputModel.PrivateKey = oRSA.Decrypt(privateKey, secretKey);
            }

            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert =
                    new System.Security.Cryptography.X509Certificates.X509Certificate2(HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                if (!string.IsNullOrEmpty(merchantId))
                    outputModel.MerchantId = oRSA.Decrypt(privateKey, merchantId);
            }

            return outputModel;
        }
    }
}
