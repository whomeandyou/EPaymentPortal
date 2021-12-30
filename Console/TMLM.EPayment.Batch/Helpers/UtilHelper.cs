using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.Batch.Helpers
{
    public static class UtilHelper
    {
        public static string MaskCardNumber(string cardNumber)
        {
            var firstDigits = cardNumber.Substring(0, 6);
            var lastDigits = cardNumber.Substring(cardNumber.Length - 4, 4);
            var requiredMask = new String('X', cardNumber.Length - firstDigits.Length - lastDigits.Length);
            return string.Concat(firstDigits, requiredMask, lastDigits);
        }
    }
}
