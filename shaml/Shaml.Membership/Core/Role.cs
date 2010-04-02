using NHibernate;
using NHibernate.Validator;
using Shaml.Core.PersistenceSupport;
using Shaml.Core.DomainModel;
using System;
using NHibernate.Validator.Constraints;
using Iesi.Collections.Generic;

namespace Shaml.Membership.Core
{
    public class Role : Entity
    {
        public Role()
        {
            Users = new HashedSet<User>();
        }

        [NotNullNotEmpty]
        public virtual string Name { get; set; }

        [NotNullNotEmpty]
        public virtual string ApplicationName { get; set; }

        public virtual ISet<User> Users { get; private set; }
    }
}
