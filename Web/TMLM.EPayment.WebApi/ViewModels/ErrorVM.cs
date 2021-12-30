using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TMLM.EPayment.WebApi.ViewModels
{
    public class ErrorVM
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string ReturnUrl { get; set; }
    }
}
