using FluentNHibernate.Conventions;
using FluentNHibernate.Mapping;

namespace WebBase.Core.Mapping
{
    public class ColumnNameConvention : IPropertyConvention
    {
        public bool Accept(IProperty propertyMap)
        {
            return propertyMap.ColumnNames.List().Count == 0;
        }

        public void Apply(IProperty propertyMap)
        {
            propertyMap.ColumnNames.Add("\"" + propertyMap.Property.Name + "\"");
        }
    }
}
