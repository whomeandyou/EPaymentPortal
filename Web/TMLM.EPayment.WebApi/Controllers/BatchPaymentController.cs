using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TMLM.EPayment.BL.Data.MPGSPayment;
using TMLM.EPayment.BL.PaymentProvider.MPGS;

namespace TMLM.EPayment.WebApi.Controllers
{
    public class BatchPaymentController : BaseController
    {
        [HttpPost,Route("BatchPayment/CallRestApiString")]
        public async Task<string> CallRestApiString(BatchPaymentRequestModel batch)
        {
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
                        RequestUri = new Uri(MPGSSettings.BASE_URL + batch.path),
                        Method = method,
                    };
                    request.Headers.Add("Authorization", $"Basic {batch.AuthorizePassword}");
                    if (method == HttpMethod.Put || method == HttpMethod.Post)
                        request.Content = new StringContent(batch.body, encoding, batch.ContentType);
                    var response = await client.SendAsync(request);
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            catch(Exception ex)
            {
                result = ex.Message;
                return result;
            }
            
            return result;
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

    }
}
