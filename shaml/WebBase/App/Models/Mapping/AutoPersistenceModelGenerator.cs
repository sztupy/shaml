using System;
using System.Linq;
using Shaml.Core.DomainModel;
using Shaml.Data.NHibernate.FluentNHibernate;
using WebBase.Core;
using Inflector.Net;
using FluentNHibernate.AutoMap;
using FluentNHibernate.MappingModel;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Discovery;
using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Mapping;

namespace WebBase.Core.Mapping
{
    public class AutoPersistenceModelGenerator : IAutoPersistenceModelGenerator
    {
        public AutoPersistenceModel Generate()
        {
            AutoPersistenceModel mappings = AutoPersistenceModel
                // If you delete the default class, simply point the following line to an entity within the .Core layer
                .MapEntitiesFromAssemblyOf<User>()
                .Where(GetAutoMappingFilter)
                .WithSetup(s => s.IsBaseType = IsBaseTypeConvention)
                .ConventionDiscovery.Add(
                  ConventionBuilder.Class.When(s => s.TableName == null, s => s.WithTable("\"" + Inflector.Net.Inflector.Capitalize(Inflector.Net.Inflector.Pluralize(s.EntityType.Name)) + "\""))
                ).ConventionDiscovery.Add(
                  ConventionBuilder.Property.When(s => s.ColumnNames.List().Count == 0,  s => s.ColumnNames.Add("\"" + s.Property.Name + "\""))
                ).ConventionDiscovery.Add(
                  DefaultLazy.AlwaysFalse()
                )
                .UseOverridesFromAssemblyOf<AutoPersistenceModelGenerator>();
            return mappings;
        }

        /// <summary>
        /// Provides a filter for only including types which inherit from the IEntityWithTypedId interface.
        /// </summary>
        private bool GetAutoMappingFilter(Type t)
        {
            return t.GetInterfaces().Any(x =>
                 x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntityWithTypedId<>));
        }

        private bool IsBaseTypeConvention(Type arg)
        {
            bool derivesFromEntity = arg == typeof(Entity);
            bool derivesFromEntityWithTypedId = arg.IsGenericType &&
                (arg.GetGenericTypeDefinition() == typeof(EntityWithTypedId<>));

            return derivesFromEntity || derivesFromEntityWithTypedId;
        }
    }
}
