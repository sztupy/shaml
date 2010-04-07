using LinFu.IoC;
using Shaml.Core.PersistenceSupport.NHibernate;
using Shaml.Data.NHibernate;
using Shaml.Core.PersistenceSupport;
using Shaml.Core.CommonValidator;
using Shaml.Core.NHibernateValidator.CommonValidatorAdapter;
using LinFu.IoC.Interfaces;
using System;
using System.IO;
using Microsoft.Practices.ServiceLocation;
using CommonServiceLocator.LinFuAdapter;
using System.Web.Mvc;
using Shaml.Web.LinFu;

namespace WebBase.Config
{
    public class ComponentRegistrar
    {
        public static void AddComponentsTo(IServiceContainer container) {
            AddGenericRepositoriesTo(container);
            AddCustomRepositoriesTo(container);
            AddApplicationServicesTo(container);

            container.AddService("validator", typeof(IValidator), typeof(Validator), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
        }

        private static void AddApplicationServicesTo(IServiceContainer container)
        {
            container.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"libraries"), "WebBase.ApplicationServices");
        }

        private static void AddCustomRepositoriesTo(IServiceContainer container)
        {
            container.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libraries"), "WebBase.Data");
            container.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libraries"), "WebBase.Core");
        }

        private static void AddGenericRepositoriesTo(IServiceContainer container)
        {
            container.AddService("entityDuplicateChecker",  typeof(IEntityDuplicateChecker), typeof(EntityDuplicateChecker), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
            container.AddService("repositoryType",  typeof(IRepository<>), typeof(Repository<>), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
            container.AddService("nhibernateRepositoryType",  typeof(INHibernateRepository<>), typeof(NHibernateRepository<>), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
            container.AddService("repositoryWithTypedId",  typeof(IRepositoryWithTypedId<,>), typeof(RepositoryWithTypedId<,>), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
            container.AddService("nhibernateRepositoryWithTypedId",  typeof(INHibernateRepositoryWithTypedId<,>), typeof(NHibernateRepositoryWithTypedId<,>), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
        }

        public static void InitializeServiceLocator()
        {
          ServiceContainer container = new ServiceContainer();
          AddComponentsTo(container);

          ControllerBuilder.Current.SetControllerFactory(new LinFuControllerFactory(container));
          ServiceLocator.SetLocatorProvider(() => new LinFuServiceLocator(container));
        }
    }
}
