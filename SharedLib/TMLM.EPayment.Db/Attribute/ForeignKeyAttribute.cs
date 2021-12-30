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

namespace TMLM.EPayment.Db.Attribute {
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : System.Attribute {
        public Type ParentTable { get; set; }
        public String ParentKeyFieldName { get; set; }

        public ForeignKeyAttribute(Type Parent, String ParentKeyField)
            : base() {
            this.ParentTable = Parent;
            this.ParentKeyFieldName = ParentKeyField;
        }
    }
}
