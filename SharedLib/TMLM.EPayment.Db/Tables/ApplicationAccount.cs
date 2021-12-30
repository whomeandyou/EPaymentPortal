
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.Db.Attribute;

namespace TMLM.EPayment.Db.Tables
{
    public class ApplicationAccount : BaseTable
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
        public string Code { get; set; }
        [TableColumn]
        public string SecretKey { get; set; }
        [TableColumn]
        public string Merchant { get; set; }
        [TableColumn]
        public string MerchantUserName { get; set; }
        [TableColumn]
        public string MerchantPassword { get; set; }
        [TableColumn]
        public string FPXSellerExchangeId { get; set; }
        [TableColumn]
        public string FPXSellerId { get; set; }
        [TableColumn]
        public string FPXSellerBankCode { get; set; }
        [TableColumn]
        public string FPXPrivateKeyPath { get; set; }
        [TableColumn]
        public string FPXPublicCertPath { get; set; }
        [TableColumn]
        public string FPXVersion { get; set; }
        [TableColumn]
        public string EMandateSellerExchangeId { get; set; }
        [TableColumn]
        public string EMandateSellerId { get; set; }
        [TableColumn]
        public string EMandateSellerBankCode { get; set; }
        [TableColumn]
        public string EMandatePrivateKeyPath { get; set; }
        [TableColumn]
        public string EMandatePublicCertPath { get; set; }
        [TableColumn]
        public string EMandateFPX_Version { get; set; }
        [TableColumn]
        public string RazerPayPublicKey { get; set; }
        [TableColumn]
        public string RazerPayPrivateKey { get; set; }
        [TableColumn]
        public string RazerPayMerchantId { get; set; }

    }
}
