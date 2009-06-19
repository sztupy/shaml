using FluentNHibernate.Conventions;
using FluentNHibernate.Mapping;

namespace WebBase.Core.Mapping
{
    public class TableNameConvention : IClassConvention
    {
        public bool Accept(IClassMap classMap) {
            return true;
        }

        public void Apply(IClassMap classMap) {
            classMap.WithTable("\"" + Inflector.Net.Inflector.Pluralize(classMap.EntityType.Name) + "\"");
        }
    }
}
