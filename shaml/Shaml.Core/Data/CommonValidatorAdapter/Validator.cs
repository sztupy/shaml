﻿using System.Reflection;
using System.Collections.Generic;
using NHibernate.Validator.Engine;
using System;
using System.Collections.ObjectModel;
using Shaml.Core.CommonValidator;

namespace Shaml.Core.NHibernateValidator.CommonValidatorAdapter
{
    /// <summary>
    /// Provides an implementation of the <see cref="CommonValidator.IValidator" /> interface 
    /// which relies on NHibernate validator
    /// </summary>
    public class Validator : Shaml.Core.CommonValidator.IValidator
    {
        static Validator()
        {
            validator = new ValidatorEngine();
        }

        public bool IsValid(object value) {
            Check.Require(value != null, "value to IsValid may not be null");

            return ValidatorEngine.IsValid(value);
        }

        public ICollection<IValidationResult> ValidationResultsFor(object value) {
            Check.Require(value != null, "value to ValidationResultsFor may not be null");

            InvalidValue[] invalidValues = ValidatorEngine.Validate(value);

            return ParseValidationResultsFrom(invalidValues);
        }

        private ICollection<IValidationResult> ParseValidationResultsFrom(InvalidValue[] invalidValues) {
            ICollection<IValidationResult> validationResults = new Collection<IValidationResult>();

            foreach (InvalidValue invalidValue in invalidValues) {
                validationResults.Add(new ValidationResult(invalidValue));
            }

            return validationResults;
        }

        private ValidatorEngine ValidatorEngine {
            get {
                return validator;
            }
        }

        private static readonly ValidatorEngine validator;
    }
}
