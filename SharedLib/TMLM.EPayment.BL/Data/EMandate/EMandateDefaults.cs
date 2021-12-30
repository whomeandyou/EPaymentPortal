using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.EPayment.BL.Data.EMandate
{
    public class EMandateDefaults
    {
        public static Dictionary<string, string> MSG_TOKEN = new Dictionary<string, string>()
        {
            { "01", "B2C (Retail Banking)" },
            { "02", "B2B (Corporate Banking)" }
        };

        public static Dictionary<string, string> FREQUENCY_MODE = new Dictionary<string, string>()
        {
            { "DL", "Daily" },
            { "WK", "Weekly" },
            { "MT", "Monthly" },
            { "YR", "Yearly" }
        };

        public static Dictionary<string, string> APPLICATION_TYPE = new Dictionary<string, string>()
        {
            { "01", "New Application" },
            { "02", "Maintenance" },
            { "03", "Termination" }
        };
    }
}
