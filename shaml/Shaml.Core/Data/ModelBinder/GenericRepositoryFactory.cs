using Shaml.Core.PersistenceSupport;
using System;

namespace Shaml.Web.ModelBinder
{
    internal class GenericRepositoryFactory
    {
        public static object CreateEntityRepositoryFor(Type entityType, Type idType) {
            Type genericRepositoryType = typeof(Shaml.Data.NHibernate.NHibernateRepositoryWithTypedId<,>);
            Type concreteRepositoryType =
                genericRepositoryType.MakeGenericType(new Type[] { entityType, idType });

            object repository;

            repository = Activator.CreateInstance(concreteRepositoryType);

            return repository;
        }
    }
}
