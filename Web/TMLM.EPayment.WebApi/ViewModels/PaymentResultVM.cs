using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Web.Mvc;

namespace TMLM.EPayment.WebApi.ViewModels
{
    public class PaymentResultVM
    {
        public string CreatedOn { get; set; }
        public string Message
        {
            get
            {
                if (!IsEMandate)
                {
                    if (AuthCode == "51")
                    {
                        return "Insuffcient Funds";
                    }
                    else
                    {
                        switch (Status)
                        {
                            case "10":
                                return "Payment has been made successfully";
                            case "5":
                                return "Payment is now pending for authorization";
                            case "1":
                                return "Payment is awaiting for payment";
                            case "98":
                                return "User cancelled payment";
                            case "97":
                                return "Transaction has invalid amount";
                            case "100":
                                return "Maximum Transaction Limit Exceeded RM30,000";
                            default:
                                return "Payment has failed";
                        }
                    }
                } else
                {
                    if (AuthCode == "51")
                    {
                        return "Insuffcient Funds";
                    }
                    else
                    {
                        switch (Status)
                        {
                            case "10":
                                return "Enrolment has been made successfully";
                            case "5":
                                return "Enrolment is now pending for authorization";
                            case "1":
                                return "Enrolment is awaiting for enrolment";
                            case "98":
                                return "User cancelled Enrolment";
                            case "97":
                                return "Transaction has invalid amount";
                            case "100":
                                return "Maximum Transaction Limit Exceeded RM30,000";
                            default:
                                return "Enrolment has failed";
                        }
                    }
                }
                
            }
        }
        public string OrderNo { get; set; }
        public string Amt { get; set; }
        public string AId { get; set; }
        public string Status { get; set; }
        public string MsgSign { get; set; }
        public string ReturnUrl { get; set; }
        public string Bank { get; set; }
        public string BankName { get; set; }
        public string RefNo { get; set; }
        public string AuthCode { get; set; }
        public string AuthNo { get; set; }
        public string SellerId { get; set; }
        public bool StayAtSummary { get; set; }
        public int Mode { get; set; }
        public string PaymentRef { get; set; }
        public bool IsEMandate { get; set; }
        public string MaxFrequency { get; set; }
        public string FrequencyMode { get; set; }
        public string ProductDesc { get; set; }
        public string ApplicationType { get; set; }
        public string DirectDebitAmount { get; set; }
        public string LABankKey { get; set; }
        public string LABankName { get; set; }
    }
}
