using FluentNHibernate.Automapping;
using WebBase.Core;
using Shaml.Data.NHibernate.FluentNHibernate;
using FluentNHibernate.Automapping.Alterations;

namespace WebBase.Data.Mapping
{
    public class UserMap : IAutoMappingOverride<User>
    {
        public void Override(AutoMapping<User> mapping)
        {
            mapping.Id(x => x.Id)
                .Column("\"pId\"")
                .GeneratedBy.UuidString();
        }
    }
}
