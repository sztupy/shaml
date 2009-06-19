using NHibernate;

namespace Shaml.Data.NHibernate
{
    public interface ISessionStorage
    {
        ISession Session { get; set; }
        string FactoryKey { get; }
    }
}
