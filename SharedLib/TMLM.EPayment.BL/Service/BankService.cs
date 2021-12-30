using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.Common;
using TMLM.EPayment.BL.Data.FPXPayment;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.BL.Service.Payment
{
    public class BankService : IDisposable
    {

        bool disposed = false;

        public BankService() { }

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

        public GetBankListOutputModel GetBankList()
        {
            using (var _repoBankList = new BankListRepository())
            {
                var bankList = _repoBankList.GetAllBankList();

                var bankListOutputModel = new GetBankListOutputModel
                {
                    Code = ResponseReturnCode.Gen_Success,
                    BankList = new List<GetBankList>()
                };

                bankListOutputModel.BankList = bankList.Select(x => new GetBankList
                {
                    BankCode = x.BankCode,
                    BankName = x.BankName
                }).ToList();

                return bankListOutputModel;
            }
        }

        public MPGSBinBankList GetMPGSBinBankNameList(string binCode)
        {
            using (var _repoMPGSBinBankList = new MPGSBinBankListRepository())
            {
                var binBankNameList = _repoMPGSBinBankList.GetMPGSBinBankListbyBinCode(binCode);

                return binBankNameList;
            }
        }

    }
}
