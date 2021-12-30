using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.Db.Repositories
{
    public class MPGSBinBankListRepository : BaseRepository
    {
        public MPGSBinBankListRepository() : this(DBUtils.ConnectionString) { }

        public MPGSBinBankListRepository(string ConnStr) : base(ConnStr) { }

        public MPGSBinBankListRepository(IDbConnection dbConn) : base(dbConn) { }

        public MPGSBinBankList GetMPGSBinBankListbyBinCode(string BinCode)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@BinCode", BinCode, DbType.String, ParameterDirection.Input);

                return base.DbConnection.Query<MPGSBinBankList>("spGet_MPGSBinCodeBankList", _dParams,
                   commandType: CommandType.StoredProcedure)
                   .FirstOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }


    }
}
