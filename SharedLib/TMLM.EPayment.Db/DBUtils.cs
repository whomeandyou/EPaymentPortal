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
using System.Reflection;
using TMLM.EPayment.Db.Tables;
using TMLM.EPayment.Db.Attribute;

namespace TMLM.EPayment.Db {
    public class DBUtils {
        public static string ConnectionString = "";
        public static string ConnectionString_Rpt = "";
        public static string ConnectionString_Trx = "";
        public static string ConnectionString_ODS = "";

        public static string ConcatAllField(PropertyInfo[] propField) {
            string _strReturn = "";
            for (int i = 0; i < propField.Length; i++) {
                if (!System.Attribute.IsDefined(propField[i], typeof(TableColumnAttribute))) {
                    continue;
                }

                string _strFieldaName = DBUtils.GetFieldName(propField[i]);
                if (string.IsNullOrEmpty(_strFieldaName))
                    continue;

                if (i != 0) {
                    _strReturn += ", ";
                }
                _strReturn += _strFieldaName;
            }
            return _strReturn;
        }

        public static bool IsColumnExist(string ColumnName, Type type) {
            return (type.GetProperties().Where(p => p.Name.ToUpper() == ColumnName.ToUpper()).Count() > 0);
        }

        public static string GetFieldName(PropertyInfo field) {
            if (!System.Attribute.IsDefined(field, typeof(TableColumnAttribute))) {
                return null;
            }

            TableColumnAttribute tcAttr = (TableColumnAttribute)System.Attribute.GetCustomAttribute(field, typeof(TableColumnAttribute));
            if (string.IsNullOrEmpty(tcAttr.FieldName)) {
                return field.Name;
            } else {
                return tcAttr.FieldName;
            }
        }
    }
}
