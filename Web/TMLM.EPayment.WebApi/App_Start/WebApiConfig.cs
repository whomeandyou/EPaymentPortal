/*********************************************************************************
 *       
 *                                                                               
 * This file is part of TMLM Portal project.                                 
 * Unauthorized copying of this file or any of the part is strictly prohibited.  
 * Proprietary and confidential                                                  
 *                                                                               
 * Written by Teong Wah, Feb 2019                
 *                                                                               
 *********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace TMLM.EPayment.WebApi {
    public static class WebApiConfig {
        public static void Register(HttpConfiguration config) {
            // Web API configuration and services
            // System.Web.Http.Cors.EnableCorsAttribute cors = new System.Web.Http.Cors.EnableCorsAttribute(
                                                                    //"agency.tokiomarinelife.com.my, banca.tokiomarinelife.com.my"
                                                                    //, "Origin, Content-Type, Accept"
                                                                    //,"GET, PUT, POST, DELETE, OPTIONS");
            System.Web.Http.Cors.EnableCorsAttribute cors = new System.Web.Http.Cors.EnableCorsAttribute("*",
                                           "Origin, Content-Type, Accept",
                                           "GET, PUT, POST, DELETE, OPTIONS");
            config.EnableCors(cors);

            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "{controller}/{action}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

#if !DEBUG
            //TW:: Adding custom attribute on HTTPS authorization 
            config.Filters.Add(new TMLM.EPayment.WebApi.Attribute.RequireHttpsAttribute());
#else
#endif

            //TW:: Adding custom logging for each request
            //config.MessageHandlers.Add(new TMLM_LogHandler());
            config.MessageHandlers.Add(new TMLM_TimeOutHandler());
            //TW:: Adding custom response handler for each request
            //config.MessageHandlers.Add(new TMLM_ResponseHandler());
        }
    }
}
