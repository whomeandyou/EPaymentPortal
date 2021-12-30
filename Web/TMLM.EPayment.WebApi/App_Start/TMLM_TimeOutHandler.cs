using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json;
using Polly.Fallback;
using Polly.Timeout;
using Polly.Wrap;

namespace TMLM.EPayment.WebApi
{
    public static class HttpRequestExtensions
    {
        private static string TimeoutPropertyKey = "RequestTimeout";

        public static void SetTimeout(this HttpRequestMessage request, TimeSpan? timeout)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Properties[TimeoutPropertyKey] = timeout;
        }

        public static TimeSpan? GetTimeout(this HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var value = (Object)null;
            TimeSpan timeout;
            if (request.Properties.TryGetValue(TimeoutPropertyKey, out value) && value is TimeSpan)
                return timeout = (TimeSpan)value;
            return null;
        }
    }

    public class TMLM_TimeOutHandler : DelegatingHandler
    {
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(15);

        protected async override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            using (System.Threading.CancellationTokenSource cts = GetCancellationTokenSource(request, cancellationToken))
            {
                try
                {
                    return await base.SendAsync(request, cts?.Token ?? cancellationToken);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException();
                }
            }
        }

        private System.Threading.CancellationTokenSource GetCancellationTokenSource(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            TimeSpan timeout = request.GetTimeout() ?? DefaultTimeout;
            if (timeout == System.Threading.Timeout.InfiniteTimeSpan)
            {
                return null;
            }
            else
            {
                System.Threading.CancellationTokenSource cts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeout);
                return cts;
            }
        }
    }
}