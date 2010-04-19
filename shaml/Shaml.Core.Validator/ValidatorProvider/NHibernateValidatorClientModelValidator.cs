using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using NHibernate.Validator.Constraints;
using NHibernate.Validator.Engine;

namespace Shaml.Core.NHibernateValidator.ValidatorProvider
{
    public class NHibernateValidatorClientModelValidator : ModelValidator
    {
        private readonly IClassValidator _validator;
        private readonly ValidatorEngine _engine;
        private string PropertyName;

        public NHibernateValidatorClientModelValidator(ModelMetadata metadata, ControllerContext controllerContext, IClassValidator validator, ValidatorEngine engine)
            : base(metadata, controllerContext)
        {
            _engine = engine;
            _validator = validator;
            this.PropertyName = metadata.PropertyName;
        }

        // We don't need server side property validation here
        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            return Enumerable.Empty<ModelValidationResult>();
        }

        /// <summary>
        /// Create client validators
        /// </summary>
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            IList<ModelClientValidationRule> rules = new List<ModelClientValidationRule>();
            IEnumerable<Attribute> validators = _validator.GetMemberConstraints(PropertyName);
            foreach (Attribute attr in validators)
            {
               /* try
                {*/
                    object instance = Activator.CreateInstance(Metadata.ContainerType);
                    if (attr is NotNullAttribute)
                    {
                        var a = attr as NotNullAttribute;
                        var message = a.Message;
                        rules.Add(new ModelClientValidationRequiredRule(message));
                    }
                    if (attr is NotNullNotEmptyAttribute)
                    {
                        var a = attr as NotNullNotEmptyAttribute;
                        var message = a.Message;
                        rules.Add(new ModelClientValidationRequiredRule(message));
                    }
                    if (attr is NotEmptyAttribute)
                    {
                        var a = attr as NotEmptyAttribute;
                        var message = a.Message;
                        rules.Add(new ModelClientValidationRequiredRule(message));
                    }
                    if (attr is RangeAttribute)
                    {
                        var a = attr as RangeAttribute;
                        var message = a.Message;
                        rules.Add(new ModelClientValidationRangeRule(message, a.Min, a.Max));
                    }
                    if (attr is LengthAttribute)
                    {
                        var a = attr as LengthAttribute;
                        var message = a.Message;
                        rules.Add(new ModelClientValidationStringLengthRule(message, a.Min, a.Max));
                    }
                    if (attr is PatternAttribute)
                    {
                        var a = attr as PatternAttribute;
                        var message = a.Message;
                        rules.Add(new ModelClientValidationRegexRule(message, a.Regex));
                    }
               /* }
                catch (Exception)
                {
                    // Message extraction failed
                }*/
            }
            return rules;
        }
    }
}
