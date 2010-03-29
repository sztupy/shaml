using NUnit.Framework;
using Microsoft.Practices.ServiceLocation;
using CommonServiceLocator.LinFuAdapter;
using Shaml.Core.PersistenceSupport.NHibernate;
using NHibernate.Validator.Engine;
using NHibernate.Validator;
using Shaml.Core.DomainModel;
using Shaml.Core;
using System.Diagnostics;
using System;
using LinFu.IoC;
using Shaml.Core.PersistenceSupport;
using Shaml.Core.NHibernateValidator;
using System.Collections.Generic;
using IValidator = Shaml.Core.CommonValidator.IValidator;
using Shaml.Core.CommonValidator;
using Shaml.Core.NHibernateValidator.CommonValidatorAdapter;

namespace Tests.Shaml.Core.CommonValidator.NHibernateValidator
{
    [TestFixture]
    public class HasUniqueObjectSignatureValidatorTests
    {
        [SetUp]
        public void SetUp() {
            InitServiceLocatorInitializer();
        }

        public void InitServiceLocatorInitializer() {
            ServiceContainer container = new ServiceContainer();

            container.LoadFromBaseDirectory("Shaml.Data.dll");
            container.AddService("duplicateChecker", typeof(IEntityDuplicateChecker), typeof(DuplicateCheckerStub), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);
            container.AddService("validator", typeof(IValidator), typeof(Validator), LinFu.IoC.Configuration.LifecycleType.OncePerRequest);

            ServiceLocator.SetLocatorProvider(() => new LinFuServiceLocator(container));
        }

        [Test]
        public void CanVerifyThatDuplicateExistsDuringValidationProcess() {
            Contractor contractor = new Contractor() { Name = "Codai" };
            IEnumerable<IValidationResult> invalidValues = contractor.ValidationResults();

            Assert.That(contractor.IsValid(), Is.False);

            foreach (IValidationResult invalidValue in invalidValues) {
                Debug.WriteLine(invalidValue.Message);
            }
        }

        [Test]
        public void CanVerifyThatNoDuplicateExistsDuringValidationProcess() {
            Contractor contractor = new Contractor() { Name = "Some unique name" };
            Assert.That(contractor.IsValid());
        }

        [Test]
        public void CanVerifyThatDuplicateExistsOfEntityWithStringIdDuringValidationProcess() {
            User user = new User() { SSN = "123-12-1234" };
            Assert.That(user.IsValid(), Is.False);
        }

        [Test]
        public void CanVerifyThatDuplicateExistsOfEntityWithGuidIdDuringValidationProcess() {
            ObjectWithGuidId objectWithGuidId = new ObjectWithGuidId() { Name = "codai" };
            Assert.That(objectWithGuidId.IsValid(), Is.False);

            objectWithGuidId = new ObjectWithGuidId() { Name = "whatever" };
            Assert.That(objectWithGuidId.IsValid(), Is.True);
        }

        [Test]
        public void MayNotUseValidatorWithEntityHavingDifferentIdType() {
            ObjectWithStringIdAndValidatorForIntId invalidCombination = 
                new ObjectWithStringIdAndValidatorForIntId() { Name = "whatever" };

            Assert.Throws<PreconditionException>(
                () => invalidCombination.ValidationResults()
            );
        }

        public class DuplicateCheckerStub : IEntityDuplicateChecker
        {
            public bool DoesDuplicateExistWithTypedIdOf<IdT>(IEntityWithTypedId<IdT> entity) {
                Check.Require(entity != null);

                if (entity as Contractor != null) {
                    Contractor contractor = entity as Contractor;
                    return !string.IsNullOrEmpty(contractor.Name) && contractor.Name.ToLower() == "codai";
                }
                else if (entity as User != null) {
                    User user = entity as User;
                    return !string.IsNullOrEmpty(user.SSN) && user.SSN.ToLower() == "123-12-1234";
                }
                else if (entity as ObjectWithGuidId != null) {
                    ObjectWithGuidId objectWithGuidId = entity as ObjectWithGuidId;
                    return !string.IsNullOrEmpty(objectWithGuidId.Name) && objectWithGuidId.Name.ToLower() == "codai";
                }

                // By default, simply return false for no duplicates found
                return false;
            }
        }

        [HasUniqueDomainSignature]
        private class Contractor : Entity
        {
            [DomainSignature]
            public string Name { get; set; }
        }

        [HasUniqueDomainSignatureWithStringId]
        private class User : EntityWithTypedId<string>
        {
            [DomainSignature]
            public string SSN { get; set; }
        }

        [HasUniqueDomainSignatureWithGuidId]
        private class ObjectWithGuidId : EntityWithTypedId<Guid>
        {
            [DomainSignature]
            public string Name { get; set; }
        }

        [HasUniqueDomainSignature]
        private class ObjectWithStringIdAndValidatorForIntId : EntityWithTypedId<string>
        {
            [DomainSignature]
            public string Name { get; set; }
        }
    }
}
