﻿using FluentNHibernate.Conventions;
using FluentNHibernate.Mapping;

namespace WebBase.Data.Mapping.Conventions {
    public class TableNameConvention : IClassConvention {
        public void Apply(FluentNHibernate.Conventions.Instances.IClassInstance instance)
        {
            instance.Table(Inflector.Net.Inflector.Pluralize(instance.EntityType.Name));
        }
    }
}
