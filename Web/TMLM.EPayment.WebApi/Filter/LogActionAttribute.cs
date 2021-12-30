
using log4net;
using System;
using System.Web.Http.ModelBinding;
using System.Web.Mvc;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace TMLM.EPayment.Web.Filters
{
    public class LogActionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var controller = filterContext.RequestContext.RouteData.Values["Controller"];
            var action = filterContext.RequestContext.RouteData.Values["Action"];

            var message = "Controller : " + controller + Environment.NewLine + "Action : " + action;

            message += Environment.NewLine + Environment.NewLine + "QueryString" + Environment.NewLine;

            foreach (var key in filterContext.HttpContext.Request.QueryString.AllKeys)
                message += key + "=" + filterContext.HttpContext.Request.QueryString[key] + Environment.NewLine;

            using (var stream = new MemoryStream())
            {
                var httpRequest = filterContext.HttpContext.Request;
                httpRequest.InputStream.Seek(0, SeekOrigin.Begin);
                httpRequest.InputStream.CopyTo(stream);
                message += Environment.NewLine + Environment.NewLine + "Request" + Environment.NewLine + Encoding.UTF8.GetString(stream.ToArray());
            }

            //do not log view
            if (filterContext.Result is JsonResult)
            {
                var jsonResult = filterContext.Result as JsonResult;
                //only log json content
                message += Environment.NewLine + Environment.NewLine + "Response" + Environment.NewLine + JsonConvert.SerializeObject(jsonResult.Data);
            }
            else
            {
                message += Environment.NewLine + Environment.NewLine + "Response" + Environment.NewLine + "html";
            }

            LogManager.GetLogger(this.GetType()).Info(message);

            base.OnActionExecuted(filterContext);
        }
    }
}