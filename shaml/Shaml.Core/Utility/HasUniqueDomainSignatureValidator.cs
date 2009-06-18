﻿using NHibernate.Validator.Engine;
using System;
using Shaml.Core.DomainModel;
using Shaml.Core.PersistenceSupport;

namespace Shaml.Core.NHibernateValidator
{
    #region Validator for entities with Id of type int

    [AttributeUsage(AttributeTargets.Class)]
    [ValidatorClass(typeof(HasUniqueDomainSignatureValidator))]
    public class HasUniqueDomainSignatureAttribute : Attribute, IRuleArgs
    {
        public string Message {
            get { return message; }
            set { message = value; }
        }

        private string message = "Provided values matched an existing, duplicate entity";
    }

    /// <summary>
    /// Provides a class level validator for determining if the entity has a unique domain signature
    /// when compared with other entries in the database.
    /// 
    /// Due to the fact that .NET does not support generic attributes, this only works for entity 
    /// types having an Id of type int.
    /// </summary>
    public class HasUniqueDomainSignatureValidator : NHibernate.Validator.Engine.IValidator
    {
        public bool IsValid(object value) {
            IEntityWithTypedId<int> entityToValidate = value as IEntityWithTypedId<int>;
            Check.Require(entityToValidate != null,
                "This validator must be used at the class level of an " +
                "IdomainWithTypedId<int>. The type you provided was " + value.GetType().ToString());

            // Shaml Architecture: Service is always EntityDuplicateChecker
            IEntityDuplicateChecker duplicateChecker = new Shaml.Data.NHibernate.EntityDuplicateChecker();
            return ! duplicateChecker.DoesDuplicateExistWithTypedIdOf<int>(entityToValidate);
        }
    }

    #endregion

    #region Validator for entities with Id of type string

    [AttributeUsage(AttributeTargets.Class)]
    [ValidatorClass(typeof(HasUniqueDomainSignatureWithStringIdValidator))]
    public class HasUniqueDomainSignatureWithStringIdAttribute : Attribute, IRuleArgs
    {
        public string Message {
            get { return message; }
            set { message = value; }
        }

        private string message = "Provided values matched an existing, duplicate entity";
    }

    /// <summary>
    /// Provides a class level validator for determining if the entity has a unique domain signature
    /// when compared with other entries in the database.
    /// 
    /// Due to the fact that .NET does not support generic attributes, this only works for entity 
    /// types having an Id of type string.
    /// </summary>
    public class HasUniqueDomainSignatureWithStringIdValidator : NHibernate.Validator.Engine.IValidator
    {
        public bool IsValid(object value) {
            IEntityWithTypedId<string> entityToValidate = value as IEntityWithTypedId<string>;
            Check.Require(entityToValidate != null,
                "This validator must be used at the class level of an " +
                "IdomainWithTypedId<string>. The type you provided was " + value.GetType().ToString() + ". " +
                "Other validators exist for various Id types. Please open an issue with S#arp Architecture " +
                "if you need a new Id type supported; you can make your own in the meantime.");

            // Shaml Architecture: Service is always EntityDuplicateChecker
            IEntityDuplicateChecker duplicateChecker = new Shaml.Data.NHibernate.EntityDuplicateChecker();
            return ! duplicateChecker.DoesDuplicateExistWithTypedIdOf<string>(entityToValidate);
        }
    }

    #endregion

    #region Validator for entities with Id of type string

    [AttributeUsage(AttributeTargets.Class)]
    [ValidatorClass(typeof(HasUniqueDomainSignatureWithGuidIdValidator))]
    public class HasUniqueDomainSignatureWithGuidIdAttribute : Attribute, IRuleArgs
    {
        public string Message {
            get { return message; }
            set { message = value; }
        }

        private string message = "Provided values matched an existing, duplicate entity";
    }

    /// <summary>
    /// Provides a class level validator for determining if the entity has a unique domain signature
    /// when compared with other entries in the database.
    /// 
    /// Due to the fact that .NET does not support generic attributes, this only works for entity 
    /// types having an Id of type string.
    /// </summary>
    public class HasUniqueDomainSignatureWithGuidIdValidator : NHibernate.Validator.Engine.IValidator
    {
        public bool IsValid(object value) {
            IEntityWithTypedId<Guid> entityToValidate = value as IEntityWithTypedId<Guid>;
            Check.Require(entityToValidate != null,
                "This validator must be used at the class level of an " +
                "IdomainWithTypedId<string>. The type you provided was " + value.GetType().ToString() + ". " +
                "Other validators exist for various Id types. Please open an issue with S#arp Architecture " +
                "if you need a new Id type supported; you can make your own in the meantime.");

            // Shaml Architecture: Service is always EntityDuplicateChecker
            IEntityDuplicateChecker duplicateChecker = new Shaml.Data.NHibernate.EntityDuplicateChecker();
            return !duplicateChecker.DoesDuplicateExistWithTypedIdOf<Guid>(entityToValidate);
        }
    }

    #endregion
}
