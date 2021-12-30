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
    public class LARazerPayEWalletRepository : BaseRepository
    {
        public LARazerPayEWalletRepository() : this(DBUtils.ConnectionString) { }

        public LARazerPayEWalletRepository(string ConnStr) : base(ConnStr) { }

        public LARazerPayEWalletRepository(IDbConnection dbConn) : base(dbConn) { }

        public LARazerPayEWallet GetLARazerPayEWalletById(string code)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@Code", code, DbType.String, ParameterDirection.Input);

                return base.DbConnection.Query<LARazerPayEWallet>("spGet_LARazerPayEWallet_By_Id", _dParams, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
