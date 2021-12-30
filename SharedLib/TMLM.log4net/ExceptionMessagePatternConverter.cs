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

/*********************************************************************************
 * This file is part of TMLM portal project.                                 
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
using System.IO;
using log4net.Core;
using log4net.Layout.Pattern;

namespace TMLM.CustomLog4Net
{
    public class ExceptionMessagePatternConverter : PatternLayoutConverter {
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent) {
            if (loggingEvent.ExceptionObject == null) {
                return;
            }
            writer.WriteLine("{0}", loggingEvent.ExceptionObject.ToString());
        }
    }
}
