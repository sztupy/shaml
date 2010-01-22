﻿using Shaml.Core.CommonValidator;
using System;
using System.Collections.Generic;

namespace Shaml.Core.DomainModel
{
    [Serializable]
    public abstract class ValidatableObject : BaseObject, IValidatable
    {
        public virtual bool IsValid() {
            return Validator.IsValid(this);
        }

        public virtual ICollection<IValidationResult> ValidationResults() {
            return Validator.ValidationResultsFor(this);
        }

        private IValidator Validator {
            get {
                return SafeServiceLocator<IValidator>.GetService();
            }
        }
    }
}
