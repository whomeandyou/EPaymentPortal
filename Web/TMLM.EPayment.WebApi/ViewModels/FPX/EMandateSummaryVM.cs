using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TMLM.EPayment.WebApi.ViewModels.FPX
{
    public class EMandateSummaryVM
    {
        public EMandateSummaryVM()
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
        public string ApplicationType { get; set; }
        public string Name { get; set; }
        public string IDNo { get; set; }
        public string MobilePhoneNo { get; set; }
        public string PurposeOfPayment { get; set; }
        public decimal MultiplyAmt { get; set; }

        public IList<SelectListItem> AvailableTransactionTypes { get; set; }
        public IList<SelectListItem> AvailableIndividualBanks { get; set; }
        public IList<SelectListItem> AvailableCorporateBanks { get; set; }
        public int Mode { get; set; }
        public string PaymentRef { get; set; }
        public int MaxFrequency { get; set; }
        public string FrequencyMode { get; set; }
        public int AmountMultiplySetting { get; set; }
        public string MsgToken { get; set; }
    }
}