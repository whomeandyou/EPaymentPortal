using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMLM.EPayment.WebApi.Attribute
{
    public class RequireHttpsAttribute : System.Web.Http.AuthorizeAttribute
    {
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (actionContext != null && 
                actionContext.Request != null &&
                !actionContext.Request.RequestUri.Scheme.Equals(Uri.UriSchemeHttps))
            {
                actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "HTTPS Required for this call"
                };
            }
            else
            {
                base.OnAuthorization(actionContext);
            }
        }
    }
}