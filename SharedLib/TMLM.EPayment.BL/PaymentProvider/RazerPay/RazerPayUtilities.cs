using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using TMLM.EPayment.BL.Data.RazerPay;
using TMLM.EPayment.Db.Repositories;
using TMLM.Security.Crytography;

namespace TMLM.EPayment.BL.PaymentProvider.RazerPay
{
    public class RazerPayUtilities
    {
        public string HashDirectStatusRequery(string orderid, string domain, string key, string amount)
        {
            //Combination of Hash = Transaction Number + Merchant_ID + Verify_Key + Amount
            var combined = $"{orderid}{domain}{key}{amount}";
            var hash = MD5Util.CalculateMD5Hash(combined).ToLower();
            return hash;
        }

        public string hashPaymentRequest(string amount, string merchantId, string orderId, string key)
        {
            //Combination of Hash = Amount + Merchant_ID + Order Id + Verify_Key
            var combined = $"{amount}{merchantId}{orderId}{key}";
            var hash = MD5Util.CalculateMD5Hash(combined).ToLower();
            return hash;
        }

        public bool VerifyRazerInquiry(string amount, string verifyKey, string domain, string transactionNum, string statCode, string secret)
        {
            var combined = $"{amount}{verifyKey}{domain}{transactionNum}{statCode}";

            var hash = MD5Util.CalculateMD5Hash(combined).ToLower();

            if (secret == hash)
                return true;
            return false;
        }

        public int GetPaymentStatus(string responseCode)
        {
            switch (responseCode)
            {
                case "10":
                    return PaymentResponseCode.Success;
                default:
                    return PaymentResponseCode.Failed;
            }
        }

        public async Task<RazerPayInquiryResponse> CallIndirectStatusRequery(string hash, string domain, string orderid, string amount)
        {
            RazerPayInquiryResponse razerPayInquiryResponse = new RazerPayInquiryResponse();
            var content = new Dictionary<string, string>();
            content.Add("amount", amount);
            content.Add("oID", orderid);
            content.Add("type", "0");
            content.Add("domain", domain);
            content.Add("skey", hash);
            content.Add("url", RazerPaySettings.InquiryReturnUrl);
            var client = new HttpClient();
            var url = RazerPaySettings.InquiryUrl;

            var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(content) };
            var res = await client.SendAsync(req);

            var resultS = await res.Content.ReadAsStringAsync();

            if (resultS.Contains("\n"))
            {
                var razerpayMessageSplit = resultS.Split(new string[] { "\r\n", "\r", "\n", "\\n" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                Dictionary<string, string> razerpayMessage = new Dictionary<string, string>();
                foreach (var split in razerpayMessageSplit)
                {
                    string[] splitString = split.Split(':');
                    razerpayMessage.Add(splitString[0], splitString[1].TrimStart());
                }

                using (var repo = new RazerPayLogRepository())
                {
                    string request = new JavaScriptSerializer().Serialize(content);
                    string response = new JavaScriptSerializer().Serialize(razerpayMessage);

                    repo.InsertRazerPayLog(orderid, "RazerPay/RequestInquiry", request, response);
                }

                razerPayInquiryResponse = DictionaryToModel<RazerPayInquiryResponse>(razerpayMessage);
                
                                                           
            }
            else
            {
                using (var repo = new RazerPayLogRepository())
                {
                    string request = new JavaScriptSerializer().Serialize(content);

                    repo.InsertRazerPayLog(orderid, "RazerPay/RequestInquiry", request, resultS);
                }
                razerPayInquiryResponse.ErrorMessage = resultS;
            }

            return razerPayInquiryResponse;
        }

        public async Task<string> UpdateThirdPartyInquiry(string statCode, string amount, string orderID, string channel, string errorCode, string errorDescription, string url)
        {
            var content = new Dictionary<string, string>();
            content.Add("statCode", statCode);
            content.Add("amount", amount);
            content.Add("orderID", orderID);
            content.Add("channel", channel);
            content.Add("errorCode", errorCode);
            content.Add("errorDescription", errorDescription);
            var client = new HttpClient();

            var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(content) };
            var res = await client.SendAsync(req);

            var resultS = await res.Content.ReadAsStringAsync();

            return resultS;
        }

        public PaymentResponseOutputModel GetPaymentResponseOutputModel(string amount, string RPTransactionNo, string orderNo, string domain, string status, string appCode, string errorCode, string errorDesc, string skey, string currency,
                                                                        string channel, string payDate, string extraParam)
        {
            PaymentResponseOutputModel model = new PaymentResponseOutputModel();
            model.Amount = amount;
            model.RazerPayTransactionNo = RPTransactionNo;
            model.OrderNo = orderNo;
            model.Domain = domain;
            model.Status = status;
            model.AppCode = appCode;
            model.ErrorCode = errorCode;
            model.ErrorDescription = errorDesc;
            model.SKey = skey;
            model.Currency = currency;
            model.Channel = channel;
            model.PayDate = payDate;
            model.ExtraParam = extraParam;
            return model;
        }

        public static T DictionaryToModel<T>(Dictionary<string, string> lem) where T : new()
        {
            T t = new T();

            foreach (var p in typeof(T).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                if (lem.TryGetValue(p.Name, out string v))
                {
                    p.SetValue(t, v);
                }
            }

            return t;
        }
    }

  
}
