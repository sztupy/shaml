using FluentNHibernate.AutoMap;

namespace Shaml.Data.NHibernate.FluentNHibernate
{
    public interface IAutoPersistenceModelGenerator
    {
        AutoPersistenceModel Generate();
    }
}
