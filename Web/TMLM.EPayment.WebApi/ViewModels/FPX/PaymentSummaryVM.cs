using System.Collections.Generic;
using System.Web.Mvc;

namespace TMLM.EPayment.WebApi.ViewModels.FPX
{
    public class PaymentSummaryVM
    {
        public PaymentSummaryVM()
        {
            AvailableIndividualBanks = new List<SelectListItem>();
            AvailableCorporateBanks = new List<SelectListItem>();
            AvailableTransactionTypes = new List<SelectListItem>();
        }
        public string MerchantName { get; set; }
        public string AId { get; set; }
        public string OrderNumber { get; set; }
        public string TransactionNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public string TotalAmount { get; set; }
        public string ReturnUrl { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerBank { get; set; }
        public string TransactionType { get; set; }

        public IList<SelectListItem> AvailableTransactionTypes { get; set; }
        public IList<SelectListItem> AvailableIndividualBanks { get; set; }
        public IList<SelectListItem> AvailableCorporateBanks { get; set; }
        public int Mode { get; set; }
        public string PaymentRef { get; set; }
    }
}
