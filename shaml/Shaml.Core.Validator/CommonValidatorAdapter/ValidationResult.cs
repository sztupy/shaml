using NHibernate.Validator.Engine;
using System;
using Shaml.Core.CommonValidator;
using Shaml.Core;

namespace Shaml.Core.Validator.CommonValidatorAdapter
{
    public class ValidationResult : Shaml.Core.CommonValidator.IValidationResult
    {
        public ValidationResult(InvalidValue invalidValue) {
            Check.Require(invalidValue != null, "invalidValue may not be null");

            ClassContext = invalidValue.EntityType;
            PropertyName = invalidValue.PropertyName;
            Message = invalidValue.Message;
            InvalidValue = invalidValue;
        }

        public virtual Type ClassContext { get; protected set; }
        public virtual string PropertyName { get; protected set; }
        public virtual string Message { get; protected set; }

        /// <summary>
        /// This is not defined by IValidationResult but is useful for applications which are 
        /// strictly using NHibernate Validator and need additional information about the 
        /// validation problem.
        /// </summary>
        public virtual InvalidValue InvalidValue { get; protected set; }
    }
}
