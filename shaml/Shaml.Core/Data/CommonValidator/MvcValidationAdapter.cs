﻿using System.Web.Mvc;
using Shaml.Core;
using System;
using System.Collections.Generic;
using Shaml.Core.CommonValidator;

namespace Shaml.Web.CommonValidator
{
    public class MvcValidationAdapter
    {
        public static ModelStateDictionary TransferValidationMessagesTo(
            ModelStateDictionary modelStateDictionary,
            IEnumerable<IValidationResult> validationResults) {

            return TransferValidationMessagesTo(null, modelStateDictionary, validationResults);
        }
    
        /// <summary>
        /// This acts as a more "manual" alternative to moving validation errors to the 
        /// <see cref="ModelStateDictionary" /> if you care to bypass the use of 
        /// <see cref="ValidatableModelBinder" />.  This typically wouldn't be used in conjunction
        /// with <see cref="ValidatableModelBinder" /> but as an alternative to it.
        /// </summary>
        /// <param name="keyBase">If supplied, will be used as the model state prefix
        /// instead of the class name</param>
        public static ModelStateDictionary TransferValidationMessagesTo(
            string keyBase, ModelStateDictionary modelStateDictionary, 
            IEnumerable<IValidationResult> validationResults) {

            Check.Require(modelStateDictionary != null, "modelStateDictionary may not be null");
            Check.Require(validationResults != null, "invalidValues may not be null");

            foreach (IValidationResult validationResult in validationResults) {
                Check.Require(validationResult.ClassContext != null,
                    "validationResult.ClassContext may not be null");

                string key = (keyBase ?? validationResult.ClassContext.Name) +
                    (!string.IsNullOrEmpty(validationResult.PropertyName)
                        ? "." + validationResult.PropertyName
                        : "");

                modelStateDictionary.AddModelError(key, validationResult.Message);
                modelStateDictionary.SetModelValue(key, new ValueProviderResult(null, null, null));
            }

            return modelStateDictionary;
        }
    }
}
