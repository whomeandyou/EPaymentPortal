using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.Batch.Model;

namespace TMLM.EPayment.Batch.Helpers
{
    public class RestApiHelper<T> : IDisposable
    {
        private readonly string Url;
        private readonly string AuthorizePassword;
        private readonly Encoding EncodingType;
        private readonly string ContentType;

        public RestApiHelper(string url, string password, string encodingType, string contenttype)
        {
            this.Url = url;
            this.AuthorizePassword = password;
            this.EncodingType = GetEncoding(encodingType);
            this.ContentType = contenttype;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        private Encoding GetEncoding(string encodingType)
        {
            Encoding _encoding = Encoding.Default;
            switch (encodingType.ToUpper())
            {
                case "ASCII":
                    _encoding = Encoding.ASCII;
                    break;
                case "BIGENDIANUNICODE":
                    _encoding = Encoding.BigEndianUnicode;
                    break;
                case "UNICODE":
                    _encoding = Encoding.Unicode;
                    break;
                case "UTF-7":
                    _encoding = Encoding.UTF7;
                    break;
                case "UTF-8":
                    _encoding = Encoding.UTF8;
                    break;
                case "UTF-32":
                    _encoding = Encoding.UTF32;
                    break;
                default:
                    break;
            }
            return _encoding;
        }

        private HttpMethod GetHttpMethod(string method)
        {
            HttpMethod _method = null;
            switch (method.ToUpper())
            {
                case "GET":
                    _method = HttpMethod.Get;
                    break;
                case "PUT":
                    _method = HttpMethod.Put;
                    break;
                case "POST":
                    _method = HttpMethod.Post;
                    break;
                case "DELETE":
                    _method = HttpMethod.Delete;
                    break;
                case "HEAD":
                    _method = HttpMethod.Head;
                    break;
                case "OPTIONS":
                    _method = HttpMethod.Options;
                    break;
                case "TRACE":
                    _method = HttpMethod.Trace;
                    break;
                default:
                    break;
            }
            return _method;
        }

        public async Task<T> CallRestApiJson(string path, HttpMethod method, string body)
        {
            T result = default(T);
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = CredentialCache.DefaultCredentials;

            using (HttpClient client = new HttpClient(handler))
            {
                HttpRequestMessage request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(Url + path),
                    Method = method
                };
                request.Headers.Add("Authorization", $"Basic {AuthorizePassword}");
                if (method == HttpMethod.Put || method == HttpMethod.Post)
                    request.Content = new StringContent(body, EncodingType, "application/json");
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<T>(content);
            }
            return result;
        }

        public async Task<string> CallRestApiString(string path, HttpMethod method, string body, string encoding)
        {
            var baseUrl = ConfigurationHelper.GetValue("centralizePortal.baseURL");
            var bypass = ConfigurationHelper.GetValue("byPassCentralizePortal");

            string result = default(string);

            BatchPaymentRequestModel requestModel = new BatchPaymentRequestModel()
            {
                path = path,
                method = method.ToString(),
                body = body,
                AuthorizePassword = AuthorizePassword,
                EncodingType = encoding,
                ContentType = ContentType
            };


            if (bypass == "true")
            {
                result = await CallMpgsApi(requestModel);
                return result;
            }

            using (var client = new HttpClient())
            {
                //set up client
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonString = JsonConvert.SerializeObject(requestModel);
                HttpResponseMessage Res = await client.PostAsync("BatchPayment/CallRestApiString", new StringContent(jsonString, Encoding.UTF8, "application/json"));
                result = await Res.Content.ReadAsStringAsync();
            }
            return result;
        }

        private async Task<string> CallMpgsApi(BatchPaymentRequestModel batch)
        {
            var mpgsBaseUrl = ConfigurationHelper.GetValue("mpgs.baseUrl");

            string result = default(string);
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = CredentialCache.DefaultCredentials;
            HttpMethod method = GetHttpMethod(batch.method);
            Encoding encoding = GetEncoding(batch.EncodingType);

            try
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(mpgsBaseUrl + batch.path),
                        Method = method,
                    };
                    request.Headers.Add("Authorization", $"Basic {batch.AuthorizePassword}");
                    if (method == HttpMethod.Put || method == HttpMethod.Post)
                        request.Content = new StringContent(batch.body, encoding, batch.ContentType);
                    var response = await client.SendAsync(request);
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
                return result;
            }

            return result;
        }

        public async Task<string> CallRestApiString(string path, HttpMethod method, string body)
        {
            string result = default(string);
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = CredentialCache.DefaultCredentials;

            using (HttpClient client = new HttpClient(handler))
            {
                HttpRequestMessage request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(Url + path),
                    Method = method,

                };
                request.Headers.Add("Authorization", $"Basic {AuthorizePassword}");
                if (method == HttpMethod.Put || method == HttpMethod.Post)
                    request.Content = new StringContent(body, EncodingType, ContentType);
                var response = await client.SendAsync(request);
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }

        public async Task<bool> PingServerAsync(string path)
        {
            bool result = false;
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.Credentials = CredentialCache.DefaultCredentials;

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{Url}{path}");
                using (HttpClient client = new HttpClient(handler))
                {
                    var response = await client.SendAsync(request);
                    var contentObject = response.Content.ReadAsStringAsync().Result;
                    if (contentObject != null && contentObject.ToString().ToUpper().Contains("OPERATING"))
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Info(String.Format("TMLM.EPayment.Batch.Helpers.RestApiHelper :=> Error: {0}, StackFlow: {1}", ex.Message, ex.StackTrace));
            }
            return result;
        }

        public void Dispose()
        {

        }
    }
}
