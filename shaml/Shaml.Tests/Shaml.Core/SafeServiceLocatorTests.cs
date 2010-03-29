using NUnit.Framework;
using System;
using Shaml.Core.CommonValidator;
using Shaml.Core;
using Microsoft.Practices.ServiceLocation;
using CommonServiceLocator.LinFuAdapter;
using Shaml.Core.NHibernateValidator.CommonValidatorAdapter;
using LinFu.IoC;

namespace Tests.Shaml.Core
{
    [TestFixture]
    public class SafeServiceLocatorTests
    {
        [SetUp]
        public void Setup()
        {
            ServiceLocator.SetLocatorProvider(null);
        }
 
        [Test]
        public void WillBeInformedIfServiceLocatorNotInitialized() {
            bool exceptionThrown = false;
 
            try {
                SafeServiceLocator<IValidator>.GetService();
            }
            catch (NullReferenceException e) {
                exceptionThrown = true;
                Assert.That(e.Message.Contains("ServiceLocator has not been initialized"));
            }
 
            Assert.That(exceptionThrown);
        }
 
        [Test]
        public void WillBeInformedIfServiceNotRegistered() {
            bool exceptionThrown = false;
 
            ServiceContainer container = new ServiceContainer();
            ServiceLocator.SetLocatorProvider(() => new LinFuServiceLocator(container));
 
            try {
                SafeServiceLocator<IValidator>.GetService();
            }
            catch (ActivationException e) {
                exceptionThrown = true;
                Assert.That(e.Message.Contains("IValidator could not be located"));
            }
 
            Assert.That(exceptionThrown);
        }
 
        [Test]
        public void CanReturnServiceIfInitializedAndRegistered() {
            ServiceContainer container = new ServiceContainer();
            container.LoadFromBaseDirectory("Shaml.Data.dll");
            container.AddService("validator", typeof(IValidator), typeof(Validator), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
 
            ServiceLocator.SetLocatorProvider(() => new LinFuServiceLocator(container));
 
            IValidator validatorService = SafeServiceLocator<IValidator>.GetService();
 
            Assert.That(validatorService, Is.Not.Null);
        }
    }
}

