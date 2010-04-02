using NHibernate;
using NHibernate.Validator;
using Shaml.Core.PersistenceSupport;
using Shaml.Core.DomainModel;
using System;
using NHibernate.Validator.Constraints;
using Iesi.Collections.Generic;

namespace Shaml.Membership.Core
{
    public class ProfileData : Entity
    {
        public ProfileData()
        {
        }

        public virtual string Name { get; set; }
        public virtual string ValueString { get; set; }
        public virtual Byte[] ValueBinary { get; set; }
    }
}