using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TMLM.EPayment.BL.Helpers
{
    public class DictHelper
    {
        public DictHelper()
        {
        }

        public static IDictionary<string, string> BuildDictFromNVC( NameValueCollection source)
        {
            return source.AllKeys.ToDictionary(k => k, k => source[k]);
        }
    }
}
