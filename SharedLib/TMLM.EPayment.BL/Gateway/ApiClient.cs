using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace TMLM.EPayment.BL.Gateway
{
    public class ApiClient
    {
        public const string POST = "POST";
        public const string GET = "GET";
        public const string PUT = "PUT";

        public string SendTransaction(ApiRequest apiRequest)
        {
            return this.executeHTTPMethod(apiRequest);
        }

        protected string executeHTTPMethod(ApiRequest apiRequest)
        {
            var body = String.Empty;

            //proxy settings
            if (apiRequest.UseProxy)
            {
                WebProxy proxy = new WebProxy(apiRequest.ProxyHost, true);
                if (!String.IsNullOrEmpty(apiRequest.ProxyUser))
                {
                    if (String.IsNullOrEmpty(apiRequest.ProxyDomain))
                    {
                        proxy.Credentials = new NetworkCredential(apiRequest.ProxyUser, apiRequest.ProxyPassword);
                    }
                    else
                    {
                        proxy.Credentials = new NetworkCredential(apiRequest.ProxyUser, apiRequest.ProxyPassword, apiRequest.ProxyDomain);
                    }
                }
                WebRequest.DefaultWebProxy = proxy;
            }

            // Create the web request
            HttpWebRequest request = WebRequest.Create(apiRequest.RequestUrl) as HttpWebRequest;

            //http verb
            request.Method = apiRequest.ApiMethod;

            //content type, json, form, etc
            request.ContentType = apiRequest.ContentType;
            request.Accept = apiRequest.Accept;

            request.AllowAutoRedirect = apiRequest.AllowAutoRedirect;
            request.KeepAlive = apiRequest.KeepAlive;
            request.Timeout = apiRequest.Timeout;

            //Logger.LogInformation($@"HttpWebRequest url {gatewayApiRequest.RequestUrl} method {request.Method}");


            //Authentication setting
            if (apiRequest.AuthenticationByCertificate)
            {
                //custom implementation fo SSL certificate validation callback
                request.ServerCertificateValidationCallback +=
                    (sender, cert, chain, error) =>
                    {
                        return error == SslPolicyErrors.None || (error != SslPolicyErrors.None && apiRequest.IgnoreSslErrors);
                    };

                //create a new certificate collection
                X509Certificate2Collection certificates = new X509Certificate2Collection();

                //load and add a new certificate loaded from file (p12) 
                certificates.Add(new X509Certificate2(new X509Certificate(apiRequest.CertificateLocation, apiRequest.CertificatePassword)));

                //attach certificate to request
                request.ClientCertificates = certificates;

            }
            else if (!string.IsNullOrEmpty(apiRequest.Username) && !string.IsNullOrEmpty(apiRequest.Password))
            {
                string credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(apiRequest.Username + ":" + apiRequest.Password));
                //string credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes("Administrator" + ":" + GatewayApiConfig.Password));
                request.Headers.Add("Authorization", "Basic " + credentials);
            }

            //build the request
            try
            {
                if ((apiRequest.ApiMethod == "PUT" || apiRequest.ApiMethod == "POST") &&
                        !String.IsNullOrEmpty(apiRequest.Payload))
                {
                    //Logger.LogInformation($@"HttpWebRequest payload {gatewayApiRequest.Payload}");

                    // Create a byte array of the data we want to send
                    byte[] utf8bytes = Encoding.UTF8.GetBytes(apiRequest.Payload);
                    byte[] iso8859bytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("iso-8859-1"), utf8bytes);

                    // Set the content length in the request headers
                    request.ContentLength = iso8859bytes.Length;

                    // Write request data
                    using (Stream postStream = request.GetRequestStream())
                    {
                        postStream.Write(iso8859bytes, 0, iso8859bytes.Length);
                    }
                }

                // Get response
                try
                {
                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        // Get the response stream
                        StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("iso-8859-1"));
                        body = reader.ReadToEnd();
                    }
                }
                catch (WebException wex)
                {
                    //Logger.LogDebug($@"Response debug : {wex.Response.Headers}");
                    StreamReader reader = new StreamReader(wex.Response.GetResponseStream(), Encoding.GetEncoding("iso-8859-1"));
                    body = reader.ReadToEnd();
                }

                //Logger.LogInformation($@"HttpWebResponse response {body}");

                return body;
            }
            catch (Exception ex)
            {
                return ex.Message + "\n\naddress:\n" + request.Address.ToString() + "\n\nheader:\n" + request.Headers.ToString() + "data submitted:\n" + apiRequest.Payload;
            }

        }

    }
}
