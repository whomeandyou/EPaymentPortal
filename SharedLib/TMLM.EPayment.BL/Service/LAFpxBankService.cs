using System;

using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.BL.Service.Payment
{
    public class LAFpxBankService : IDisposable
    {

        bool disposed = false;

        public LAFpxBankService() { }

        public FpxBank GetLAFpxBank(string fpxBankId)
        {
            using (var _repoLAFpxBank = new LAFpxBankRepository())
                return _repoLAFpxBank.GetLAFpxBankByFpxBankID(fpxBankId);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
