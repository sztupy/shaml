﻿using FluentNHibernate.Conventions;
using FluentNHibernate.Mapping;

namespace WebBase.Core.Mapping
{
    public class ColumnNameConvention : IPropertyConvention
    {
        public bool Accept(IProperty propertyMap)
        {
            return true;
        }

        public void Apply(IProperty propertyMap)
        {
            if (propertyMap.ColumnNames.List().Count == 0) {
                propertyMap.ColumnNames.Add("\"" + propertyMap.Property.Name + "\"");
            }
        }
    }
}
