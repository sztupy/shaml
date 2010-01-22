using LinFu.IoC;
using Shaml.Core.PersistenceSupport.NHibernate;
using Shaml.Data.NHibernate;
using Shaml.Core.PersistenceSupport;
using Shaml.Core.CommonValidator;
using Shaml.Core.Validator.CommonValidatorAdapter;

namespace WebBase
{
    public class ComponentRegistrar
    {
        public static void AddComponentsTo(ServiceContainer container) {
            AddGenericRepositoriesTo(container);
            AddCustomRepositoriesTo(container);
            AddApplicationServicesTo(container);

            container.AddService("validator", typeof(IValidator), typeof(Validator), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
        }

        private static void AddApplicationServicesTo(ServiceContainer container)
        {
           /* container.Register(
                AllTypes.Pick()
                .FromAssemblyNamed("WebBase.ApplicationServices")
                .WithService.FirstInterface());*/
        }

        private static void AddCustomRepositoriesTo(ServiceContainer container)
        {
            /*container.Register(
                AllTypes.Pick()
                .FromAssemblyNamed("WebBase.Data")
                .WithService.FirstNonGenericCoreInterface("WebBase.Core"));*/
        }

        private static void AddGenericRepositoriesTo(ServiceContainer container)
        {
            container.AddService("entityDuplicateChecker",
                typeof(IEntityDuplicateChecker), typeof(EntityDuplicateChecker), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
            container.AddService("repositoryType",
                typeof(IRepository<>), typeof(Repository<>), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
            container.AddService("nhibernateRepositoryType",
                typeof(INHibernateRepository<>), typeof(NHibernateRepository<>), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
            container.AddService("repositoryWithTypedId",
                typeof(IRepositoryWithTypedId<,>), typeof(RepositoryWithTypedId<,>), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
            container.AddService("nhibernateRepositoryWithTypedId",
                typeof(INHibernateRepositoryWithTypedId<,>), typeof(NHibernateRepositoryWithTypedId<,>), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
        }
    }
}
