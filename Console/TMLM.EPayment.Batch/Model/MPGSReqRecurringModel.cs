using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch.Model
{
    public class MPGSReqRecurringModel
    {
        public enum MPGSResponeseKeys
        {
            ApiOperation = 0,
            OrderId = 1,
            TransactionId = 2,
            orderReference = 3,
            TransactionReference = 4,
            OrderAmount = 5,
            OrderCurrency = 6,
            SourceOfFundsType = 7,
            SourceOfFundsProvidedCardNumber = 8,
            SourceOfFundsProvidedCardExpiryMonth = 9,
            SourceOfFundsProvidedCardExpiryYear = 10,
            TransactionSource = 11,
            SourceOfFundsProvidedCardStoredOnFile = 12,
            AgreementId = 13,
            AgreementType = 14,
            Result = 15,
            RrrorCause = 16,
            ErrorExplanation = 17,
            ResponseGatewayCode = 18,
            AuthorizationResponseResponseCode = 19,
            TransactionAuthorizationCode = 20
        }

        public string apiOperation { get; set; }
    }
}
