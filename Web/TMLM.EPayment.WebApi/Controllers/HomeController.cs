using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMLM.EPayment.WebApi.Gateway;

namespace TMLM.EPayment.WebApi.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home
        public ActionResult Index()
        {
            ViewBag.Title = "Streamline Portal API is running!!!";
            return View();
        }
        public ActionResult PaymentType()
        {
            return View();
        }
        public ActionResult Pay()
        {
            GatewayApiRequest gatewayApiRequest = GatewayApiRequest.createApiRequest(GatewayApiConfig);

            //ViewBag.JavascriptSessionUrl = getSessionJsUrl(GatewayApiConfig);
            //ViewBag.TestAndGoLiveUrl = getTestAndGoLiveDocumentationURL();

            return View(gatewayApiRequest);

        }
    }
}