
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.Db.Attribute;

namespace TMLM.EPayment.Db.Tables
{
    public class ResponseCode : BaseTable
    {
        public override System.Reflection.PropertyInfo[] TableColumns
        {
            get
            {
                return this.GetType().GetProperties().Where(
                    prop => System.Attribute.IsDefined(prop, typeof(TableColumnAttribute))).ToArray();
            }
        }

        [TableColumn]
        public int Id { get; set; }
        [TableColumn]
        public string PaymentProvider { get; set; }
        [TableColumn]
        public string Code { get; set; }
        [TableColumn]
        public string Description { get; set; }
        [TableColumn]
        public int TMLMStatus { get; set; }
    }
}
