/*********************************************************************************
 * This file is part of TMLM portal project.                                 
 * Unauthorized copying of this file or any of the part is strictly prohibited.  
 * Proprietary and confidential                                                  
 *                                                                               
 * Written by Teong Wah, Feb 2019                
 *                                                                               
 *********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;

using TMLM.Security.Crytography;

namespace TMLM.Security {
    public class AuthManager : IDisposable {
        // Flag: Has Dispose already been called?
        bool disposed = false;

        public AuthManager() {
            //this._dbContext = new AuthEntities();
        }
        // Public implementation of Dispose pattern callable by consumers.
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