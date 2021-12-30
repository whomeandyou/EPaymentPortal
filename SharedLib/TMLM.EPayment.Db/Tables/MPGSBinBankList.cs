using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.Db.Attribute;

namespace TMLM.EPayment.Db.Tables
{
    public class MPGSBinBankList : BaseTable
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
        public string Bin { get; set; }
        [TableColumn]
        public string bankcode { get; set; }
        [TableColumn]
        public string BankName { get; set; }
        [TableColumn]
        public bool IsActive { get; set; }
    }
}
