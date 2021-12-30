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
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TMLM.EPayment.WebApi
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            if (ConfigurationManager.AppSettings["DeploymentType"] == "Internal")
            {
                routes.MapRoute(
                    "internal_default",
                    "internal/{controller}/{action}/{id}",
                    new { action = "Index", id = UrlParameter.Optional }
                );
            }

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                //defaults: new { id = UrlParameter.Optional },
                //defaults: new { controller = "MPGSPayment", action = "Index", id = UrlParameter.Optional }
                defaults: new { controller = "Home", action = "PaymentType", id = UrlParameter.Optional }
            );
        }
    }
}
