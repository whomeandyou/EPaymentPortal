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
using System.Web;
using System.Web.Mvc;

namespace TMLM.EPayment.WebApi
{
    public class FilterConfig {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
