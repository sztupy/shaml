using System;
using System.Linq;
using FluentNHibernate;
using FluentNHibernate.AutoMap;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Helpers;
using Shaml.Core.DomainModel;
using Shaml.Data.NHibernate.FluentNHibernate;
using WebBase.Core;

namespace WebBase.Core.Mapping
{
    public class AutoPersistenceModelGenerator : IAutoPersistenceModelGenerator
    {
        public AutoPersistenceModel Generate() {
            AutoPersistenceModel mappings = AutoPersistenceModel
                // If you delete the default class, simply point the following line to an entity within the .Core layer
                .MapEntitiesFromAssemblyOf<User>()
                .Where(GetAutoMappingFilter)
                .ConventionDiscovery.Setup(GetConventions())
                .WithSetup(GetSetup())
                .UseOverridesFromAssemblyOf<AutoPersistenceModelGenerator>();

            return mappings;
        }

        private Action<AutoMappingExpressions> GetSetup() {
            return c => {
                c.FindIdentity = type => type.Name == "Id";
                c.IsBaseType = IsBaseTypeConvention;
            };
        }

        private Action<IConventionFinder> GetConventions() {
            return c => {
                c.Add(
                    DefaultLazy.AlwaysFalse()
                );
                c.Add<PrimaryKeyConvention>();
                c.Add<ReferenceConvention>();
                c.Add<HasManyConvention>();
                c.Add<TableNameConvention>();
                c.Add<ColumnNameConvention>();
            };
        }

        /// <summary>
        /// Provides a filter for only including types which inherit from the IEntityWithTypedId interface.
        /// </summary>
        private bool GetAutoMappingFilter(Type t) {
            return t.GetInterfaces().Any(x =>
                 x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntityWithTypedId<>));
        }

        private bool IsBaseTypeConvention(Type arg) {
            bool derivesFromEntity = arg == typeof(Entity);
            bool derivesFromEntityWithTypedId = arg.IsGenericType && 
                (arg.GetGenericTypeDefinition() == typeof(EntityWithTypedId<>));

            return derivesFromEntity || derivesFromEntityWithTypedId;
        }
    }
}