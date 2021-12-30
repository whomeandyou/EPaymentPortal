using System;
using TMLM.EPayment.BL.Data;
using TMLM.EPayment.BL.Data.PaymentProvider;

namespace TMLM.EPayment.BL.Service.PaymentProvider
{
    public interface IPaymentProcessor : IDisposable
    {
        OutputModel InitiatePayment(InitiatePaymentInputModel model);

        GetHtmlOutputModel GenerateRequestHTML(GetHtmlInputModel model);

        ProcessPaymentOutputModel ProcessPayment(ProcessPaymentInputModel model);

        OutputModel CancelPayment(string transactionNumber);

        OutputModel FailPaymentWithStatus(string transactionNumber, string status);

        InquiryPaymentOutputModel Inquiry(InquiryPaymentInputModel model);
    }
}
