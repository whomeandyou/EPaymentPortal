
namespace TMLM.EPayment.BL.Gateway
{
    public class ApiRequest
    {
        public string ApiMethod { get; set; } = "PUT";
        public string RequestUrl { get; set; }
        public string Payload { get; set; }

        public string ContentType { get; set; } = "application/json; charset=iso-8859-1";
        public string Accept { get; set; } = "application/json";
        public bool AllowAutoRedirect { get; set; }
        public bool KeepAlive { get; set; }
        public int Timeout { get; set; } = 300000;


        public bool UseSsl { get; set; }
        public bool IgnoreSslErrors { get; set; }

        //proxy configuration
        public bool UseProxy { get; set; }
        public string ProxyHost { get; set; }
        public string ProxyUser { get; set; }
        public string ProxyPassword { get; set; }
        public string ProxyDomain { get; set; }

        //backing fields - avoid get/set stackoverflow 
        public string Password;
        public string Username;
        public string CertificateLocation;
        public string CertificatePassword;


        public bool AuthenticationByCertificate
        {
            get
            {
                return CertificateLocation != null && CertificatePassword != null;
            }
        }

    }
}
