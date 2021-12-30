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
    public class LAFpxBankRepository : BaseRepository
    {
        public LAFpxBankRepository() : this(DBUtils.ConnectionString) { }

        public LAFpxBankRepository(string ConnStr) : base(ConnStr) { }

        public LAFpxBankRepository(IDbConnection dbConn) : base(dbConn) { }

        public FpxBank GetLAFpxBankByFpxBankID(string fpxBankId)
        {
            try
            {
                DynamicParameters _dParams = new DynamicParameters();
                _dParams.Add("@FpxBankId", fpxBankId, DbType.String, ParameterDirection.Input);

                return base.DbConnection.Query<FpxBank>("spGet_LAFpxBankByFpxBankID", _dParams,
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
