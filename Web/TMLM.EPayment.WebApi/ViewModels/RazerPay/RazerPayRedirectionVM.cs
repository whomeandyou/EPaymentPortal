using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMLM.EPayment.WebApi.ViewModels.RazerPay
{
    public class RazerPayRedirectionVM
    {
        public string Amount { get; set; }

        public string OrderNo { get; set; }

        public string BillName { get; set; }

        public string BillEmail { get; set; }

        public string BillMobile { get; set; }

        public string Description { get; set; }

        public string Country { get; set; }

        public string Hash { get; set; }

        public string ReturnUrl { get; set; }
    }
}