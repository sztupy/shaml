using NHibernate;
using NHibernate.Validator;
using Shaml.Core.PersistenceSupport;
using Shaml.Core.DomainModel;
using System;
using NHibernate.Validator.Constraints;

namespace WebBase.Core
{
    public class User : EntityWithTypedId<string>, IHasAssignedId<string>
    {
        public User() { }

        [DomainSignature]
        [NotNullNotEmpty]
        public virtual string Username { get; protected set; }

        [DomainSignature]
        [NotNullNotEmpty]
        public virtual string Email { get; protected set; }

        [NotNullNotEmpty]
        public virtual string Comment { get; protected set; }        

        public virtual void SetAssignedIdTo(string assignedId)
        {
            throw new NotImplementedException();
        }
    }
}
