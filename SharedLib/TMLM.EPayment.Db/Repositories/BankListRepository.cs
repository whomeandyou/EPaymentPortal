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
    public class BankListRepository : BaseRepository
    {
        public BankListRepository() : this(DBUtils.ConnectionString) { }

        public BankListRepository(string ConnStr) : base(ConnStr) { }

        public BankListRepository(IDbConnection dbConn) : base(dbConn) { }

        public List<BankList> GetAllBankList()
        {
            try
            {
                return base.DbConnection.Query<BankList>("spGet_BankList",
                   commandType: CommandType.StoredProcedure)
                   .ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }


    }
}
