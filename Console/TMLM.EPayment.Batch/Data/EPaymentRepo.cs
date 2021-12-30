using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMLM.EPayment.Batch.Model;

namespace TMLM.EPayment.Batch.Data
{
    public class EPaymentRepo : IDisposable
    {
        protected readonly SqlConnection Conn;

        public EPaymentRepo(string dbConnectionString)
        {
            this.Conn = new SqlConnection(dbConnectionString);
        }

        public IDbConnection Open()
        {
            Conn.Open();
            return Conn;
        }
        public async Task<IDbConnection> OpenAsync()
        {
            await Conn.OpenAsync();
            return Conn;
        }

        public EnrollmentStatusModel GetEnrollmentDetail(string policyNo)
        {
            DynamicParameters dParams = new DynamicParameters();
            string query = $"SELECT "
                            + " [Veres],[Pares],[AcsEci],[AuthenticationToken],[TransactionId],[dsVersion] "
                            + " FROM EnrollmentStatus WHERE PolicyNo = @PolicyNo";

            dParams.Add("@PolicyNo", policyNo);

            return this.Conn.QueryFirstOrDefault<EnrollmentStatusModel>(new CommandDefinition(query, dParams));
        }

        public void Close()
        {
            if (this.Conn.State == ConnectionState.Open)
                this.Conn.Close();
        }

        public void Dispose()
        {
            if (this.Conn.State == ConnectionState.Open)
                this.Conn.Close();

            Conn.Dispose();
        }
    }
}
