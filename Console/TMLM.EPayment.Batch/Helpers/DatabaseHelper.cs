using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TMLM.EPayment.Batch.Helpers
{
    public class DatabaseHelper
    {
        private readonly SqlConnection _sqlConn;
        private readonly string ConnectionString;
        public DatabaseHelper(string connectionString)
        {
            _sqlConn = new SqlConnection(connectionString);
            ConnectionString = connectionString;
        }

        private void Open()
        {
            _sqlConn.Open();
        }

        private void Close()
        {
            _sqlConn.Close();
        }

        public void ExecuteNonQuery(string query)
        {
            try
            {
                Open();
                SqlCommand cmd = new SqlCommand(query, _sqlConn);
                var test = cmd.ExecuteNonQuery();
                LogHelper.Info("Successfully execute the query");
                Close();
            }
            catch (Exception ex)
            {
                LogHelper.ErrorFormat(String.Format("Failed:=> sql: {0}, Error: {1}", query, ex), ex);
                Close();
            }
        }

        public int ExecuteScalar(string query)
        {
            try
            {
                Open();
                SqlCommand cmd = new SqlCommand(query, _sqlConn);
                int rtnResult = Convert.ToInt32(cmd.ExecuteScalar());
                LogHelper.Info("Successfully execute the query");
                Close();

                return rtnResult;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorFormat($"Failed to execute the query", ex);
                Close();
            }

            return 0;
        }

        public DataTable ExecuteReader(string query)
        {
            var dt = new DataTable();
            try
            {
                Open();
                SqlCommand cmd = new SqlCommand(query, _sqlConn);
                SqlDataReader reader = cmd.ExecuteReader();
                LogHelper.Info("Successfully execute the query");
                dt.Load(reader);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorFormat($"Failed to execute the query", ex);
            }
            finally{
                Close();
            }
            return dt;
        }

        public bool ExecuteScalarBool(string query)
        {
            try
            {
                bool hasRecords = false;

                Open();
                SqlCommand cmd = new SqlCommand(query, _sqlConn);
                var rdr = cmd.ExecuteReader();
                LogHelper.Info("Successfully execute the query");
                hasRecords = rdr.HasRows; 

                Close();
                return hasRecords;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorFormat($"Failed to execute the query : {query}", ex);
                Close();
                return false;
            }
        }

        public int ExecuteSPNon(string query, List<SqlParameter> sqlParameters = null)
        {
            int rtnCount = 0;

            try
            {
                Open();
                SqlCommand cmd = new SqlCommand(query, _sqlConn);
                cmd.CommandType = CommandType.Text;

                if (sqlParameters != null)
                    cmd.Parameters.AddRange(sqlParameters.ToArray());

                rtnCount = cmd.ExecuteNonQuery();
                Close();
                return rtnCount;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorFormat($"Failed to execute the query : {query}", ex);
                Close();
                return 0;
            }
        }
    }    
}
