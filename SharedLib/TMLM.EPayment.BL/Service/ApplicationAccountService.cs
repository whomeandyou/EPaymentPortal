
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Web;
using System.Net.Http;
using System.IO;
using System.Transactions;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

using TMLM;
using TMLM.Common;
using System.Security.Cryptography;
using TMLM.EPayment.Db.Repositories;
using TMLM.EPayment.Db.Tables;

namespace TMLM.EPayment.BL.Service.Payment
{
    public class ApplicationAccountService : IDisposable
    {
        // Flag: Has Dispose already been called?
        bool disposed = false;

        public ApplicationAccountService() { }

        public ApplicationAccount GetApplicationAccountById(int id)
        {
            ApplicationAccount applicationAccount = null;
            using (var _repoApplicationAccount = new ApplicationAccountRepository())
            {
                applicationAccount = _repoApplicationAccount.GetApplicationAccountById(id);
                string mapPath = System.Web.Hosting.HostingEnvironment.MapPath("~/");
                applicationAccount.EMandatePrivateKeyPath = Path.Combine(mapPath, applicationAccount.EMandatePrivateKeyPath);
                applicationAccount.EMandatePublicCertPath = Path.Combine(mapPath, applicationAccount.EMandatePublicCertPath);
                applicationAccount.FPXPrivateKeyPath = Path.Combine(mapPath, applicationAccount.FPXPrivateKeyPath);
                applicationAccount.FPXPublicCertPath = Path.Combine(mapPath, applicationAccount.FPXPublicCertPath);
            }
            return applicationAccount;
        }

        public ApplicationAccount GetApplicationAccountByCode(string code)
        {
            ApplicationAccount applicationAccount = null;
            using (var _repoApplicationAccount = new ApplicationAccountRepository())
            {
                applicationAccount = _repoApplicationAccount.GetApplicationAccountByCode(code);
                string mapPath = System.Web.Hosting.HostingEnvironment.MapPath("~/");
                applicationAccount.EMandatePrivateKeyPath = Path.Combine(mapPath, applicationAccount.EMandatePrivateKeyPath);
                applicationAccount.EMandatePublicCertPath = Path.Combine(mapPath, applicationAccount.EMandatePublicCertPath);
                applicationAccount.FPXPrivateKeyPath = Path.Combine(mapPath, applicationAccount.FPXPrivateKeyPath);
                applicationAccount.FPXPublicCertPath = Path.Combine(mapPath, applicationAccount.FPXPublicCertPath);
            }
            return applicationAccount;
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
