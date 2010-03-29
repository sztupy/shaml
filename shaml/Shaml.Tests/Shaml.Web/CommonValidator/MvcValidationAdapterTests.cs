﻿using NUnit.Framework;
using System.Web.Mvc;
using Shaml.Web.NHibernate;
using System.Collections.Generic;
using Shaml.Core.CommonValidator;
using Shaml.Core.NHibernateValidator.CommonValidatorAdapter;
using NHibernate.Validator.Engine;
using Shaml.Web.CommonValidator;

namespace Tests.Shaml.Web.CommonValidator
{
    [TestFixture]
    public class MvcValidationAdapterTests
    {
        [Test]
        public void CanTransferValidationMessagesToModelState() {
            ModelStateDictionary modelStateDictionary = new ModelStateDictionary();
            IList<IValidationResult> invalidValues = new List<IValidationResult>();
            invalidValues.Add(
                new ValidationResult(
                    new InvalidValue("Message 1", typeof(TransactionAttribute), "Property1", "Test 1", null, null)));
            invalidValues.Add(
                new ValidationResult(
                    new InvalidValue("Message 2", typeof(MvcValidationAdapter), "Property2", "Test 2", null, null)));
            invalidValues.Add(
                new ValidationResult(
                    new InvalidValue("Message 3", GetType(), "Property3", "Test 3", null, null)));

            MvcValidationAdapter.TransferValidationMessagesTo(modelStateDictionary, invalidValues);

            Assert.That(modelStateDictionary.Count, Is.EqualTo(3));
            Assert.That(modelStateDictionary["TransactionAttribute.Property1"].Errors[0].ErrorMessage,
                Is.EqualTo("Message 1"));
			Assert.That(modelStateDictionary["TransactionAttribute.Property1"].Value.AttemptedValue,
				Is.EqualTo("Test 1"));
            Assert.That(modelStateDictionary["MvcValidationAdapter.Property2"].Errors[0].ErrorMessage,
                Is.EqualTo("Message 2"));
			Assert.That(modelStateDictionary["MvcValidationAdapter.Property2"].Value.AttemptedValue,
				Is.EqualTo("Test 2"));
            Assert.That(modelStateDictionary["MvcValidationAdapterTests.Property3"].Errors[0].ErrorMessage,
                Is.EqualTo("Message 3"));
			Assert.That(modelStateDictionary["MvcValidationAdapterTests.Property3"].Value.AttemptedValue,
				Is.EqualTo("Test 3"));
        }
    }
}
