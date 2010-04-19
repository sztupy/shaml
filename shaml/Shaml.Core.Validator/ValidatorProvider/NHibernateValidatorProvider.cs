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
        /// Returns model validators for each class that can be validated.
        /// When this method is called with a non-class modelType, nothing is added to the yield return
        /// (this prevents us from validating the same properties several times)
        /// </summary>
        public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
        {
            var classValidator = ValidatorEngine.GetClassValidator(metadata.ModelType);

            if (classValidator != null)
            {
                yield return new NHibernateValidatorModelValidator(metadata, context, classValidator);
            }
            else
            {
                // For client validation
                classValidator = ValidatorEngine.GetClassValidator(metadata.ContainerType);
                if (classValidator != null)
                {
                    yield return new NHibernateValidatorClientModelValidator(metadata, context, classValidator, ValidatorEngine);
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
