using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Db.Tables
{
    public class LARazerPayEWallet
    {
        public int Id { get; set; }
        public string EWalletId { get; set; }
        public string LAEWalletKey { get; set; }
        public string EWalletName { get; set; }
        public int IsActive { get; set; }
    }
}
