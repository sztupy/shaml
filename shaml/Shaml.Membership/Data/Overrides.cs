using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Automapping;
using Shaml.Membership.Core;

namespace Shaml.Membership.Data
{
    public class Overrides
    {
        static public void ProfileDataOverride(AutoMapping<ProfileData> mapping)
        {
            mapping.Map(x => x.ValueString).CustomType("StringClob").CustomSqlType("text");
        }
        static public void RoleOverride(AutoMapping<Role> mapping)
        {
            mapping.Map(x => x.Name).UniqueKey("roles_rolename_application_unique");
            mapping.Map(x => x.ApplicationName).UniqueKey("roles_rolename_application_unique");
        }
        static public void SessionOverride(AutoMapping<Session> mapping)
        {
            mapping.Map(x => x.Data).CustomType("StringClob").CustomSqlType("text");
            mapping.Map(x => x.SessionId).UniqueKey("sessions_sessionid_unique");
        }
        static public void UserOverride(AutoMapping<User> mapping)
        {
            mapping.Map(x => x.Username).UniqueKey("users_username_application_unique");
            mapping.Map(x => x.ApplicationName).UniqueKey("users_username_application_unique");

            mapping.Map(x => x.Username).UniqueKey("users_username_application_email_unique");
            mapping.Map(x => x.ApplicationName).UniqueKey("users_username_application_email_unique");
            mapping.Map(x => x.Email).UniqueKey("users_username_application_email_unique");

            mapping.Map(x => x.Email).Index("users_email_index");
            mapping.Map(x => x.IsLockedOut).Index("users_islockedout_index");
        }
    }
}
