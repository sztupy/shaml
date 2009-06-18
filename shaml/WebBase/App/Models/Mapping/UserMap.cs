using FluentNHibernate.AutoMap;
using WebBase.Core;
using Shaml.Data.NHibernate.FluentNHibernate;
using FluentNHibernate.AutoMap.Alterations;

namespace WebBase.Core.Mapping
{
    public class UserMap : IAutoMappingOverride<User>
    {
        public void Override(AutoMap<User> mapping)
        {
            mapping.Id(x => x.Id)
                .ColumnName("\"pId\"")
                .GeneratedBy.UuidString();
        }
    }
}
