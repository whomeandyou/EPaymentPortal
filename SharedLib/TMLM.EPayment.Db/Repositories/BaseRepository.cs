/*********************************************************************************
 *       
 *                                                                               
 * This file is part of TMLM Portal project.                                 
 * Unauthorized copying of this file or any of the part is strictly prohibited.  
 * Proprietary and confidential                                                  
 *                                                                               
 * Written by Teong Wah, Feb 2019                
 *                                                                               
 *********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

using Dapper;
using System.Data.SqlClient;

namespace TMLM.EPayment.Db.Repositories {
    public abstract class BaseRepository : IDisposable {
        public IDbConnection DbConnection;
        protected bool dbRef;
        protected IDbTransaction DbTransaction;
        // Flag: Has Dispose already been called?
        bool disposed = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ConnStr"></param>
        /// <exception cref="ApplicationException"></exception>
        protected BaseRepository(string ConnStr) {
            if (string.IsNullOrEmpty(ConnStr)) {
                throw new ApplicationException("The connection string is null.");
            }
            this.DbConnection = DbConnectionFactory.OpenConnection(ConnStr);
            this.dbRef = false;
        }

        protected BaseRepository(IDbConnection dbConn) {
            this.DbConnection = dbConn;
            this.dbRef = true;
        }

        protected static void SetIdentity<T>(IDbConnection connection, Action<T> setId) {
            dynamic identity = connection.Query("SELECT @@IDENTITY AS Id").Single();
            T newId = (T)identity.Id;
            setId(newId);
        }

        public void BeginTrans() {
            if (this.DbTransaction == null) {
                this.DbTransaction = this.DbConnection.BeginTransaction();
            }
        }

        public void Execute(string sql, object Param) {
            this.DbConnection.Execute(sql, Param, this.DbTransaction);
        }

        public List<T> ExecuteSP<T>(string sql, object Param) {
            return this.DbConnection.Query<T>(sql, Param, this.DbTransaction, true, null, CommandType.StoredProcedure).ToList();
        }

        public void ExecuteSPNon(string sql, object Param) {
            this.DbConnection.Execute(sql, Param, this.DbTransaction, null, CommandType.StoredProcedure);
        }

        public void CommitTrans() {
            if (this.DbTransaction != null) {
                this.DbTransaction.Commit();
                this.DbTransaction.Dispose();
                this.DbTransaction = null;
            }
        }

        public void RollbackTrans() {
            if (this.DbTransaction != null) {
                this.DbTransaction.Rollback();
                this.DbTransaction.Dispose();
                this.DbTransaction = null;
            }
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (!this.dbRef) {
                this.RollbackTrans();

                //this is to make sure, this instance was the first instance which create the db connection
                this.DbConnection.Close();
                this.DbConnection.Dispose();
            }
            // Free any unmanaged objects here.
            //
            disposed = true;
        }
    }
}
