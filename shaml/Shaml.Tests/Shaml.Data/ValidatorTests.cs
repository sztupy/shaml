using NUnit.Framework;
using NHibernate.Validator.Constraints;
using Shaml.Core.CommonValidator;
using System.Collections.Generic;
using System.Linq;
using Shaml.Core.Validator.CommonValidatorAdapter;

namespace Tests.Shaml.Core.NHibernateValidator.CommonValidatorAdapter
{
    [TestFixture]
    public class ValidatorTests
    {
        [Test]
        public void CanValidateObject() {
            Validator validator = new Validator();

            SomeObject invalidObject = new SomeObject();
            Assert.That(validator.IsValid(invalidObject), Is.False);

            SomeObject validObject = new SomeObject() {
                Name = ""
            };
            Assert.That(validator.IsValid(validObject), Is.True);
        }

        [Test]
        public void CanRetriveValiationResults() {
            Validator validator = new Validator();

            SomeObject invalidObject = new SomeObject();
            ICollection<IValidationResult> results = validator.ValidationResultsFor(invalidObject);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().PropertyName, Is.EqualTo("Name"));
            Assert.That(results.First().ClassContext, Is.EqualTo(typeof(SomeObject)));
            Assert.That(results.First().Message, Is.EqualTo("Dude...the name please!!"));
        }

        private class SomeObject
        {
            [NotNull(Message="Dude...the name please!!")]
            public string Name { get; set; }
        }
    }
}
