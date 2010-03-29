using LinFu.IoC;
using Shaml.Core.CommonValidator;
using Shaml.Core.NHibernateValidator.CommonValidatorAdapter;
using CommonServiceLocator.LinFuAdapter;
using Microsoft.Practices.ServiceLocation;
using Shaml.Core.PersistenceSupport;
using Tests.WebBase.Data.TestDoubles;

namespace Tests
{
    public class ServiceLocatorInitializer
    {
        public static void Init() {
            ServiceContainer container = new ServiceContainer();
            container.AddService("validator", typeof(IValidator), typeof(Validator));
            container.AddService("entityDuplicateChecker", typeof(IEntityDuplicateChecker), typeof(EntityDuplicateCheckerStub));
            ServiceLocator.SetLocatorProvider(() => new LinFuServiceLocator(container));
        }
    }
}
