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
using System.Text;
using System.Threading.Tasks;

namespace TMLM.Common
{
    public class ExceptionUtil {
        public static Exception GetRootException(Exception ex) {
            if (ex.InnerException == null)
                return ex;

            return ExceptionUtil.GetRootException(ex.InnerException);
        }
    }
}
