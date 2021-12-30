using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using TMLM.Security.Crytography;

namespace ToEncryptConfig
{
    class Program
    {
        static void Main(string[] args)
        {
            /*-- using RSA --*/
            string connStr, result;
            string CAcertPath = @"C:\project\EpaymentPortal\EpaymentPortal\Web\TMLM.EPayment.WebApi\TMLM.pfx";
            
            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                X509Certificate2 CAcert = new X509Certificate2(CAcertPath, "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;
                System.Security.Cryptography.AsymmetricAlgorithm publicKey = CAcert.PublicKey.Key;

                /*-- connection string --*/
                connStr = oRSA.Encrypt(publicKey, "927a5042338f6ecbd9e0b0fb056f9ca5");
               // connStr = oRSA.Encrypt(publicKey, "Data Source=10.180.1.105;Initial Catalog=Epayment_testRazarpay;User ID=sa;Password=P@ssw0rd;");
                //connStr = oRSA.Encrypt(publicKey, "RHBtokiomarine_Dev"); ;
                //connStr = oRSA.Encrypt(publicKey, @"SERVER=10.180.1.64\AgencyDB,1434;Initial Catalog=StarterPack;User ID=webuser;Password=P@ssw0rd123;"); 
                result = oRSA.Decrypt(privateKey, "ifJeTOL+wTvW0Wr7OqV4jA9y4oUUDANSl9bjuJTZl8lXyAWyvznuYLt9cgbwwBQBaYjKz3bpR//whm66R9fGlnFAovuZI7MqmbYVRMB90i+EaYbzcNXFNNUPZlKt6gflw5B60P/T8OglZiHtw/MXg12SYzErRroB5euz2R1VyVxM+j5X7Z8mK8VFqWzZTUrZsaGD4fC3MSroWM9c037HgB/QFlda1FbkHcBUsYwinXirLX9leKlsXcD18lzN40SwL16eLxQiRbyA+pYMOgSAnPPwhSbhxlacyUxPYemwiP0/kyVv+c6FkxfLQxHg10eG5SKFfK1u2ktuGA+inePgug==");
                Console.WriteLine("Sql Connection String");
                Console.WriteLine(connStr);
                Console.WriteLine(result);
            }

            //MPGS.apiPassword
            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                X509Certificate2 CAcert = new X509Certificate2(CAcertPath, "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;
                System.Security.Cryptography.AsymmetricAlgorithm publicKey = CAcert.PublicKey.Key;

                /*-- connection string --*/
                connStr = oRSA.Encrypt(publicKey, "kl1631");
                
                connStr = oRSA.Encrypt(publicKey, "4f17194f579f49cad82645ff9654c59f");
              
                result = oRSA.Decrypt(privateKey, connStr);

                Console.WriteLine("");
                Console.WriteLine("MPGS.apiPassword");
                Console.WriteLine(connStr);
                Console.WriteLine(result);
            }

            //mpgs.apiusername
            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                X509Certificate2 CAcert = new X509Certificate2(CAcertPath, "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;
                System.Security.Cryptography.AsymmetricAlgorithm publicKey = CAcert.PublicKey.Key;

                /*-- connection string --*/
                /* <add name="DBMSsqlConn" connectionString="Data Source=10.180.1.25;Initial Catalog=DBMS_CustomerPortal_SIT;User ID=dlrmuser;Password=1q2w3e4r5t;Connect Timeout=120;"/> */
                connStr = oRSA.Encrypt(publicKey, "merchant.TEST001918501503");
                result = oRSA.Decrypt(privateKey, connStr);

                Console.WriteLine("");
                Console.WriteLine("MPGS.apiusername");
                Console.WriteLine(connStr);
                Console.WriteLine(result);
            }

            //mpgs.merchant
            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                X509Certificate2 CAcert = new X509Certificate2(CAcertPath, "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;
                System.Security.Cryptography.AsymmetricAlgorithm publicKey = CAcert.PublicKey.Key;

                /*-- connection string --*/
                connStr = oRSA.Encrypt(publicKey, "TEST001918501503");
                result = oRSA.Decrypt(privateKey, connStr);

                Console.WriteLine("");
                Console.WriteLine("MPGS.merchant");
                Console.WriteLine(connStr);
                Console.WriteLine(result);
            }

            //FPX.SellerExchangeId
            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                X509Certificate2 CAcert = new X509Certificate2(CAcertPath, "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;
                System.Security.Cryptography.AsymmetricAlgorithm publicKey = CAcert.PublicKey.Key;

                /*-- connection string --*/
                connStr = oRSA.Encrypt(publicKey, "EX00007852");
                result = oRSA.Decrypt(privateKey, connStr);

                Console.WriteLine("");
                Console.WriteLine("FPX.SellerExchangeId");
                Console.WriteLine(connStr);
                Console.WriteLine(result);
            }

            //FPX.SellerId
            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                X509Certificate2 CAcert = new X509Certificate2(CAcertPath, "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;
                System.Security.Cryptography.AsymmetricAlgorithm publicKey = CAcert.PublicKey.Key;

                /*-- connection string --*/
                connStr = oRSA.Encrypt(publicKey, "SE00011908");
                result = oRSA.Decrypt(privateKey, connStr);

                Console.WriteLine("");
                Console.WriteLine("FPX.SellerId");
                Console.WriteLine(connStr);
                Console.WriteLine(result);
            }

            if(Console.ReadKey(true).Key == ConsoleKey.Enter)
            {
                System.Environment.Exit(0);
            }
        }
    }
}
