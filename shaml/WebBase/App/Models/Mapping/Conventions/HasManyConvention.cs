﻿using FluentNHibernate.Conventions;
using FluentNHibernate.Mapping;

namespace WebBase.Core.Mapping
{
    public class HasManyConvention : IHasManyConvention
    {
        public bool Accept(IOneToManyPart oneToManyPart) {
            return true;
        }

        public void Apply(IOneToManyPart oneToManyPart) {
            oneToManyPart.KeyColumnNames.Clear();
            oneToManyPart.KeyColumnNames.Add(oneToManyPart.EntityType.Name + "Fk");
        }
    }
}