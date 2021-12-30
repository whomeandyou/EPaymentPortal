using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.Db.Repositories;

namespace TMLM.EPayment.BL.Service
{
    public class MPGSLogService : IDisposable
    {
        bool disposed = false;
        public MPGSLogService() { }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void InsertMPGSLog(string orderNo, string action, string request, string response)
        {
            using (var _repoApplicationAccount = new MPGSLogRepository())
                 _repoApplicationAccount.InsertMPGSLog(orderNo,action, request,response);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            // Free any unmanaged objects here.
            if (disposing)
            {
                //stuff to dispose
            }
            disposed = true;
        }

    }
}
