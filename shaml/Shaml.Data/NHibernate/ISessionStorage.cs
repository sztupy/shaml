using NHibernate;
using System.Collections.Generic;

namespace Shaml.Data.NHibernate
{
    public interface ISessionStorage
    {
		ISession GetSessionForKey(string factoryKey);
		void SetSessionForKey(string factoryKey, ISession session);
		IEnumerable<ISession> GetAllSessions();
    }
}
