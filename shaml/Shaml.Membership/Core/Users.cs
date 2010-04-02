using NHibernate;
using NHibernate.Validator;
using Shaml.Core.PersistenceSupport;
using Shaml.Core.DomainModel;
using System;
using NHibernate.Validator.Constraints;
using Iesi.Collections.Generic;

namespace Shaml.Membership.Core
{
    public class User : Entity
    {
        public User() {
            Roles = new HashedSet<Role>();
            OpenIdAlternatives = new HashedSet<string>();
        }

        [NotNullNotEmpty]
        public virtual string Username { get; set; }

        [NotNullNotEmpty]
        public virtual string Email { get; set; }

        [NotNullNotEmpty]
        public virtual string Comment { get; set; }

        [NotNullNotEmpty]
        public virtual string ApplicationName { get; set;  }

        public virtual string Password { get; set; }
        public virtual string PasswordQuestion { get; set; }
        public virtual string PasswordAnswer { get; set; }
        public virtual bool IsApproved { get; set; }
        public virtual DateTime LastActivityDate { get; set; }
        public virtual DateTime LastLoginDate { get; set; }
        public virtual DateTime LastPasswordChangedDate { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual bool IsOnline { get; set; }
        public virtual bool IsLockedOut { get; set; }
        public virtual DateTime LastLockedOutDate { get; set; }
        public virtual int FailedPasswordAttemptCount { get; set; }
        public virtual DateTime FailedPasswordAttemptWindowStart { get; set; }
        public virtual int FailedPasswordAnswerAttemptCount { get; set; }
        public virtual DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }

        public virtual ISet<string> OpenIdAlternatives { get; set; }
        public virtual ISet<Role> Roles { get; set; }
        public virtual Profile UserProfile { get; set; }

        public virtual void AddRole(Role role)
        {
            Roles.Add(role);
            role.Users.Add(this);
        }

        public virtual bool RemoveRole(Role role)
        {
            Roles.Remove(role);
            return role.Users.Remove(this);
        }

        public virtual void SetProfile(Profile profile)
        {
            UserProfile = profile;
            UserProfile.User = this;
        }
    }
}
