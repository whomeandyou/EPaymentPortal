/*********************************************************************************
 *       
 *                                                                               
 * This file is part of TMLM Portal project.                                 
 * Unauthorized copying of this file or any of the part is strictly prohibited.  
 * Proprietary and confidential                                                  
 *                                                                               
 *                                                                               
 *********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Configuration;
using System.IO;
using TMLM.EPayment.Db;

using log4net;
using TMLM.EPayment.BL.PaymentProvider.FPX;
using TMLM.EPayment.BL.Service.Payment;
using TMLM.EPayment.BL.PaymentProvider.MPGS;
using TMLM.EPayment.BL.Service.Authentication;
using TMLM.EPayment.BL.PaymentProvider.RazerPay;

namespace TMLM.EPayment.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            MPGSSettings.BASE_URL = ConfigurationManager.AppSettings["MPGS.gatewayURL"];
            MPGSSettings.VERSION = ConfigurationManager.AppSettings["MPGS.version"];
            MPGSSettings.CURRENCY = ConfigurationManager.AppSettings["MPGS.currency"];

         

            MPGSSettings.API_OPERATION = ConfigurationManager.AppSettings["MPGS.apiOperation"];
            MPGSSettings.API_OPERATION_PAYER_AUTHENTICATE = ConfigurationManager.AppSettings["MPGS.apiOperationPayerAuthenticate"];
            MPGSSettings.CREATE_SESSION = ConfigurationManager.AppSettings["MPGS.CreateSession"];
            MPGSSettings.API_OPERATION_ACS_PROCESS = ConfigurationManager.AppSettings["MPGS.apiOperationACSProcess"];
            MPGSSettings.API_OPERATION_PAY = ConfigurationManager.AppSettings["MPGS.apiOperationPAY"];
            MPGSSettings.API_OPERATION_VERIFY = ConfigurationManager.AppSettings["MPGS.apiOperationVerify"];
            MPGSSettings.API_OPERATION_VOID = ConfigurationManager.AppSettings["MPGS.apiOperationVoid"];
            MPGSSettings.Enable3DS2 = Convert.ToBoolean(ConfigurationManager.AppSettings["MPGS.Enable3DS2"]);

            PaymentSettings.BASE_URL = ConfigurationManager.AppSettings["PaymentApi.BaseUrl"];

            FPXSettings.BASE_URL = ConfigurationManager.AppSettings["FPX.BaseUrl"];
            EMandateSettings.BASE_URL = ConfigurationManager.AppSettings["EMandate.BaseUrl"];
            EMandateSettings.AMT_MULTIPLICATION = ConfigurationManager.AppSettings["EMandate.Amt_Multiply"];


            RazerPaySettings.BASE_URL = ConfigurationManager.AppSettings["RazerPay.BaseUrl"];
            RazerPaySettings.InquiryReturnUrl = ConfigurationManager.AppSettings["RazerPay.InquiryReturnUrl"];
            RazerPaySettings.InquiryUrl = ConfigurationManager.AppSettings["RazerPay.InquiryUrl"];











            DBUtils.ConnectionString_ODS = ConfigurationManager.ConnectionStrings["ODSsqlConn"].ConnectionString;
            using (TMLM.Security.Crytography.RSA oRSA = new TMLM.Security.Crytography.RSA())
            {
                System.Security.Cryptography.X509Certificates.X509Certificate2 CAcert
                = new System.Security.Cryptography.X509Certificates.X509Certificate2
                    (HttpRuntime.AppDomainAppPath + @"\TMLM.pfx", "1q2w3e4r5t");
                System.Security.Cryptography.AsymmetricAlgorithm privateKey = CAcert.PrivateKey;

                DBUtils.ConnectionString = oRSA.Decrypt(privateKey,
                    ConfigurationManager.ConnectionStrings["sqlConn"].ConnectionString);
                CAcert = null;
            }








            /*-- setup for TLS 1.2 --*/
            if (!System.Net.ServicePointManager.SecurityProtocol.HasFlag(System.Net.SecurityProtocolType.Tls12))
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.ServicePointManager.SecurityProtocol | System.Net.SecurityProtocolType.Tls12;
            }
            /*-- get rid of the X-AspNet-Version header --*/
            MvcHandler.DisableMvcResponseHeader = true;

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //-- configure log4net --
            log4net.Config.XmlConfigurator.Configure(new FileInfo(HttpRuntime.AppDomainAppPath + @"log4net.config"));


            log4net.Repository.Hierarchy.Hierarchy hierarchy = log4net.LogManager.GetRepository() as log4net.Repository.Hierarchy.Hierarchy;
            if (hierarchy != null && hierarchy.Configured)
            {
                foreach (log4net.Appender.IAppender appender in hierarchy.GetAppenders())
                {
                    if (appender is log4net.Appender.AdoNetAppender)
                    {
                        log4net.Appender.AdoNetAppender adoNetAppender = (log4net.Appender.AdoNetAppender)appender;
                        adoNetAppender.ConnectionString = DBUtils.ConnectionString;
                        adoNetAppender.ActivateOptions(); //Refresh AdoNetAppenders Settings
                    }
                }
            }
        }

        /// <summary>
        /// Handle application error on a global level.
        /// Passes error handling off to the ErrorController
        /// </summary>
        protected void Application_Error() { }

        protected void Application_PreSendRequestHeaders()
        {
            // removing excessive headers. They don't need to see this.
            /*-- alter response header --*/
            Response.Headers.Remove("server");
            Response.Headers.Remove("x-aspnet-version");
            Response.Headers.Remove("x-powered-by");
        }
        protected void Application_EndRequest()
        {
            // removing excessive headers. They don't need to see this.
            /*-- alter response header --*/
            Response.Headers.Remove("server");
            Response.Headers.Remove("x-aspnet-version");
            Response.Headers.Remove("x-powered-by");
        }

        //to handle the preflight Options requests with HTTP OPTIONS requests.
        protected void Application_BeginRequest()
        {
            if (Request.Headers.AllKeys.Contains("Origin") && Request.HttpMethod == "OPTIONS")
            {
                Response.Flush();
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// This method execute when session time expired
        /// </summary>
        protected void Session_End(object sender, EventArgs e)
        {
            // reset terminal login token
            // check existing value into session
            if ((Object)Session["SessionUserID"] != null)
            {
                //TokenReset objTokenReset = new TokenReset();
                //objTokenReset.LoginTokenReset(Session["SessionUserID"].ToString());
                Session["SessionUserID"] = null;
            }
        }
    }
    public class WebConfig
    {
        public static int ExpiredInterval
        {
            get
            {
                string _strTemp = ConfigurationManager.AppSettings["ExpiredInterval"];
                if (string.IsNullOrEmpty(_strTemp))
                {
                    //if no configure, default to 6 hours;
                    return 6;
                }
                else
                {
                    try
                    {
                        int _iReturn = 6;
                        int.TryParse(_strTemp, out _iReturn);
                        return _iReturn;
                    }
                    catch
                    {
                        return 6;
                    }
                }
            }
        }
    }
}
