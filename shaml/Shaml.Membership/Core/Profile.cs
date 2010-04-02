using NHibernate;
using NHibernate.Validator;
using Shaml.Core.PersistenceSupport;
using Shaml.Core.DomainModel;
using System;
using NHibernate.Validator.Constraints;
using Iesi.Collections.Generic;
using System.Collections.Generic;

namespace Shaml.Membership.Core
{
    public class Profile : Entity
    {
        public Profile()
        {
           Data = new HashedSet<ProfileData>();
        }

        [NotNullNotEmpty]
        public virtual string ApplicationName { get; set; }

        public virtual bool IsAnonymous { get; set; }
        public virtual DateTime LastActivityDate { get; set; }
        public virtual DateTime LastUpdatedDate { get; set; }

        public virtual User User { get; set; }
        public virtual ISet<ProfileData> Data { get; set; }
        
        public virtual void AddProfileData(ProfileData pd) {
            Data.Add(pd);
        }

        public virtual bool RemoveProfileData(ProfileData pd) {
            return Data.Remove(pd);
        }

    }
}
