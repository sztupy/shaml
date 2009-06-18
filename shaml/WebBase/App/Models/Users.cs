using NHibernate.Validator;
using Shaml.Core.PersistenceSupport;
using Shaml.Core.DomainModel;

using System;

namespace WebBase.Core
{
    public class User : EntityWithTypedId<string>, IHasAssignedId<string>
    {
        public User() { }

        [DomainSignature]
        [NotNull, NotEmpty]
        public virtual string Username { get; protected set; }

        [DomainSignature]
        [NotNull, NotEmpty]
        public virtual string Email { get; protected set; }

        public virtual void SetAssignedIdTo(string assignedId)
        {
            throw new NotImplementedException();
        }
    }
}