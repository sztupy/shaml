using System;
using System.Linq;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Conventions;
using WebBase.Core;
using WebBase.Data.Mapping.Conventions;
using Shaml.Core.DomainModel;
using Shaml.Data.NHibernate.FluentNHibernate;

namespace WebBase.Data.Mapping
{

    public class AutoPersistenceModelGenerator : IAutoPersistenceModelGenerator
    {

        #region IAutoPersistenceModelGenerator Members

        public AutoPersistenceModel Generate()
        {
            var mappings = new AutoPersistenceModel();
            mappings.AddEntityAssembly(typeof(WebSample).Assembly).Where(GetAutoMappingFilter);
            mappings.Conventions.Setup(GetConventions());
            mappings.Setup(GetSetup());
            mappings.IgnoreBase<Entity>();
            mappings.IgnoreBase(typeof(EntityWithTypedId<>));
            mappings.UseOverridesFromAssemblyOf<AutoPersistenceModelGenerator>();
            
            // Membershipprovider Automappings
            mappings.AddEntityAssembly(typeof(Shaml.Membership.Core.User).Assembly).Where(GetAutoMappingFilter);
            mappings.Override<Shaml.Membership.Core.User>(map => Shaml.Membership.Data.Overrides.UserOverride(map));
            mappings.Override<Shaml.Membership.Core.Role>(map => Shaml.Membership.Data.Overrides.RoleOverride(map));
            mappings.Override<Shaml.Membership.Core.ProfileData>(map => Shaml.Membership.Data.Overrides.ProfileDataOverride(map));
            mappings.Override<Shaml.Membership.Core.Session>(map => Shaml.Membership.Data.Overrides.SessionOverride(map));

            return mappings;

        }

        #endregion

        private Action<AutoMappingExpressions> GetSetup()
        {
            return c =>
            {
                c.FindIdentity = type => type.Name == "Id";
                c.IsComponentType = type => type.BaseType == typeof(ValueObject);
            };
        }

        private Action<IConventionFinder> GetConventions()
        {
            return c =>
            {
                c.Add<WebBase.Data.Mapping.Conventions.ForeignKeyConvention>();
                c.Add<WebBase.Data.Mapping.Conventions.HasManyConvention>();
                c.Add<WebBase.Data.Mapping.Conventions.HasManyToManyConvention>();
                c.Add<WebBase.Data.Mapping.Conventions.ManyToManyTableNameConvention>();
                c.Add<WebBase.Data.Mapping.Conventions.PrimaryKeyConvention>();
                c.Add<WebBase.Data.Mapping.Conventions.ReferenceConvention>();
                c.Add<WebBase.Data.Mapping.Conventions.TableNameConvention>();
                c.Add<WebBase.Data.Mapping.Conventions.EnumConvention>();                
            };
        }

        /// <summary>
        /// Provides a filter for only including types which inherit from the IEntityWithTypedId interface.
        /// </summary>

        private bool GetAutoMappingFilter(Type t)
        {
            return t.GetInterfaces().Any(x =>
                                         x.IsGenericType &&
                                         x.GetGenericTypeDefinition() == typeof(IEntityWithTypedId<>));
        }
    }
}
