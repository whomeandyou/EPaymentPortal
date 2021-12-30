using System.Configuration;

namespace TMLM.EPayment.Batch.Helpers
{
    public static class ConfigurationHelper
    {
        public static string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
