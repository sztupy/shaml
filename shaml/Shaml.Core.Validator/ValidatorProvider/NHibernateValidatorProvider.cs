using System.Collections.Generic;
using NHibernate.Validator.Engine;
using Shaml.Data.NHibernate;
using Shaml.Core.NHibernateValidator.ValidatorProvider;
using System.Web.Mvc;
using System.Reflection;

namespace Shaml.NHibernateValidator.ValidatorProvider
{
    public class NHibernateValidatorProvider : ModelValidatorProvider
    {
        static NHibernateValidatorProvider()
        {
            validator = NHibernateSession.ValidatorEngine ?? new ValidatorEngine();
        }

        /// <summary>
        /// Returns model validators for each class that can be validated, and client validators for each properties that
        /// can be validated.
        /// </summary>
        public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
        {
            var classValidator = ValidatorEngine.GetClassValidator(metadata.ModelType);

            // Server side validation
            if (classValidator != null)
            {
                yield return new NHibernateValidatorModelValidator(metadata, context, classValidator);
            }

            // Client side validation
            if (metadata.ContainerType != null)
            {
                var propertyValidator = ValidatorEngine.GetClassValidator(metadata.ContainerType);
                if (propertyValidator != null)
                {
                    yield return new NHibernateValidatorClientModelValidator(metadata, context, propertyValidator, ValidatorEngine);
                }
            }
        }

        private ValidatorEngine ValidatorEngine
        {
            get
            {
                return validator;
            }
        }

        private static readonly ValidatorEngine validator;
    }
}
