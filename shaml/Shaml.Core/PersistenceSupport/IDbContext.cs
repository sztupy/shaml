using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shaml.Core.PersistenceSupport
{
    /// <summary>
    /// Note that outside of CommitChanges(), you shouldn't have to invoke this object very often.  
    /// If you're using the <see cref="Shaml.Web.NHibernate.TransactionAttribute"/> on your 
    /// controller actions, then the transaction opening/committing will be taken care of for you.
    /// </summary>
    public interface IDbContext
    {
        void CommitChanges();
        void CommitChanges(bool clearChanges);
        IDisposable BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}
