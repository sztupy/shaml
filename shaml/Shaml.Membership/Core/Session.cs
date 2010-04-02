using NHibernate;
using NHibernate.Validator;
using Shaml.Core.PersistenceSupport;
using Shaml.Core.DomainModel;
using System;
using NHibernate.Validator.Constraints;
using Iesi.Collections.Generic;

namespace Shaml.Membership.Core
{
    public class Session : Entity
    {
        public Session()
        {
        }

        [NotNullNotEmpty]
        public virtual string ApplicationName { get; set; }

        public virtual string SessionId { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime Expires { get; set; }
        public virtual int Timeout { get; set; }
        public virtual bool Locked { get; set; }
        public virtual int LockId { get; set; }
        public virtual DateTime LockDate { get; set; }
        public virtual string Data { get; set; }
        public virtual int Flags { get; set; }
    }
}