using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.Common
{
    [Serializable]
    public class TMLMWebAPIException : Exception {
        public String ErrCode { get; set; }

        public TMLMWebAPIException(string ErrCode, string ErrMsg)
            : base(ErrMsg) {
            this.ErrCode = ErrCode;
        }
    }
}
