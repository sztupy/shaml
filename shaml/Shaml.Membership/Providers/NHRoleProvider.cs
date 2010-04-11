//
// $Id$
//
// Copyright © 2006 - 2008 Nauck IT KG		http://www.nauck-it.de
//
// Author:
//	Daniel Nauck		<d.nauck(at)nauck-it.de>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Hosting;
using System.Web.Security;
using NHibernate;
using Shaml.Membership.Core;
using Shaml.Data.NHibernate;
using NHibernate.Criterion;
using Shaml.Membership;


namespace Shaml.Membership
{
  public class NHRoleProvider : RoleProvider
  {
    private const string s_rolesTableName = "Roles";
    private const string s_userInRolesTableName = "UsersInRoles";
    //private string m_connectionString = string.Empty;

    /// <summary>
    /// System.Configuration.Provider.ProviderBase.Initialize Method
    /// </summary>
    public override void Initialize(string name, NameValueCollection config)
    {
      // Initialize values from web.config.
      if (config == null)
        throw new ArgumentNullException("config");

      if (string.IsNullOrEmpty(name))
        name = "NHRoleProvider";

      if (string.IsNullOrEmpty(config["description"]))
      {
        config.Remove("description");
        config.Add("description", "NHRoleProvider");
      }

      // Initialize the abstract base class.
      base.Initialize(name, config);

      m_applicationName = NHOpenIDMembershipProvider.GetConfigValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);

      // Get connection string.
      //m_connectionString = NHOpenIDMembershipProvider.GetConnectionString(config["connectionStringName"]);
    }

    /// <summary>
    /// System.Web.Security.RoleProvider properties.
    /// </summary>
    #region System.Web.Security.RoleProvider properties
    private string m_applicationName = string.Empty;

    public override string ApplicationName
    {
      get { return m_applicationName; }
      set { m_applicationName = value; }
    }
    #endregion

    /// <summary>
    /// System.Web.Security.RoleProvider methods.
    /// </summary>
    #region System.Web.Security.RoleProvider methods

    /// <summary>
    /// RoleProvider.AddUsersToRoles
    /// </summary>
    public override void AddUsersToRoles(string[] usernames, string[] roleNames)
    {
      foreach (string rolename in roleNames)
      {
        if (!RoleExists(rolename))
        {
          throw new ProviderException(string.Format(CultureInfo.InvariantCulture, "Role does not exist", rolename));
        }
      }

      foreach (string username in usernames)
      {
        foreach (string rolename in roleNames)
        {
          if (IsUserInRole(username, rolename))
          {
            throw new ProviderException(string.Format(CultureInfo.InvariantCulture, "User already in role", username, rolename));
          }
        }
      }

      using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
      {
        using (s.BeginTransaction())
        {
          foreach (string username in usernames)
          {
            var usercrit = s.CreateCriteria<User>();
            usercrit.Add(Expression.Eq("ApplicationName", m_applicationName));
            usercrit.Add(Expression.Eq("Username", username));
            var user = usercrit.UniqueResult<User>();
            if (user != null)
            {
              foreach (string rolename in roleNames)
              {
                var rolecrit = s.CreateCriteria<Role>();
                rolecrit.Add(Expression.Eq("ApplicationName", m_applicationName));
                rolecrit.Add(Expression.Eq("Name", rolename));
                var role = rolecrit.UniqueResult<Role>();
                if (role != null)
                {
                  user.AddRole(role);
                }
              }
            }
          }
          s.Transaction.Commit();
        }
      }
    }

    /// <summary>
    /// RoleProvider.CreateRole
    /// </summary>
    public override void CreateRole(string roleName)
    {
      if (RoleExists(roleName))
      {
        throw new ProviderException(string.Format(CultureInfo.InvariantCulture, "RoleAlreadyExist {1}", roleName));
      }
      using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
      {
        using (s.BeginTransaction())
        {
          Role r = new Role();
          r.Name = roleName;
          r.ApplicationName = m_applicationName;
          s.Save(r);
          s.Transaction.Commit();
        }
      }
    }

    /// <summary>
    /// RoleProvider.DeleteRole
    /// </summary>
    public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
    {
      if (!RoleExists(roleName))
      {
        throw new ProviderException(string.Format(CultureInfo.InvariantCulture, "ErrRoleNotExist {1}", roleName));
      }

      if (throwOnPopulatedRole && GetUsersInRole(roleName).Length > 0)
      {
        throw new ProviderException("ErrCantDeletePopulatedRole");
      }

      using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
      {
        using (s.BeginTransaction())
        {
          var c = s.CreateCriteria<Role>();
          c.Add(Expression.Eq("Name", roleName));
          c.Add(Expression.Eq("ApplicationName", m_applicationName));
          var role = c.UniqueResult<Role>();
          if (role != null)
          {
            s.Delete(role);
            s.Transaction.Commit();
          }
          else return false;
        }
      }

      return true;
    }

    /// <summary>
    /// RoleProvider.FindUsersInRole
    /// </summary>
    public override string[] FindUsersInRole(string roleName, string usernameToMatch)
    {
      List<string> userList = new List<string>();

      using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
      {
        using (s.BeginTransaction())
        {
          var c = s.CreateCriteria<User>();
          c.CreateCriteria("Roles", "r");
          c.Add(Expression.Eq("r.Name", roleName));
          c.Add(Expression.Eq("r.ApplicationName", m_applicationName));
          c.Add(Expression.InsensitiveLike("Username", usernameToMatch));
          var list = c.List<User>();
          foreach (var user in list)
          {
            userList.Add(user.Username);
          }
        }
      }

      return userList.ToArray();
    }

    /// <summary>
    /// RoleProvider.GetAllRoles
    /// </summary>
    public override string[] GetAllRoles()
    {
      List<string> rolesList = new List<string>();

      using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
      {
        using (s.BeginTransaction())
        {
          var c = s.CreateCriteria<Role>();
          c.Add(Expression.Eq("ApplicationName", m_applicationName));
          var list = c.List<Role>();
          foreach (var role in list)
          {
            rolesList.Add(role.Name);
          }
        }
      }
      return rolesList.ToArray();
    }

    /// <summary>
    /// RoleProvider.GetRolesForUser
    /// </summary>
    public override string[] GetRolesForUser(string username)
    {
      List<string> rolesList = new List<string>();

      using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
      {
        using (s.BeginTransaction())
        {
          var c = s.CreateCriteria<User>();
          c.Add(Expression.Eq("Username", username));
          c.Add(Expression.Eq("ApplicationName", m_applicationName));
          c.SetFetchMode("Roles", FetchMode.Join);
          var user = c.UniqueResult<User>();
          if (user != null)
          {
            foreach (var role in user.Roles)
            {
              rolesList.Add(role.Name);
            }
          }
        }
      }
      return rolesList.ToArray();
    }

    /// <summary>
    /// RoleProvider.GetUsersInRole
    /// </summary>
    public override string[] GetUsersInRole(string roleName)
    {
      List<string> userList = new List<string>();

      using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
      {
        using (s.BeginTransaction())
        {
          var c = s.CreateCriteria<Role>();
          c.Add(Expression.Eq("Name", roleName));
          c.Add(Expression.Eq("ApplicationName", m_applicationName));
          c.SetFetchMode("Users", FetchMode.Join);
          var role = c.UniqueResult<Role>();
          if (role != null)
          {
            foreach (var user in role.Users)
            {
              userList.Add(user.Username);
            }
          }
        }
      }
      return userList.ToArray();
    }

    /// <summary>
    /// RoleProvider.IsUserInRole
    /// </summary>
    public override bool IsUserInRole(string username, string roleName)
    {
      //List<string> userList = new List<string>();

      using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
      {
        using (s.BeginTransaction())
        {
          var c = s.CreateCriteria<Role>();
          c.Add(Expression.Eq("Name", roleName));
          c.Add(Expression.Eq("ApplicationName", m_applicationName));
          c.CreateCriteria("Users", "u");
          c.Add(Expression.Eq("u.Username", username));
          c.SetFetchMode("Users", FetchMode.Join);
          var role = c.UniqueResult<Role>();
          if (role != null)
          {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// RoleProvider.RemoveUsersFromRoles
    /// </summary>
    public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
    {
      foreach (string rolename in roleNames)
      {
        if (!RoleExists(rolename))
        {
          throw new ProviderException(string.Format(CultureInfo.InvariantCulture, "ErrRoleNotExist {1}", rolename));
        }
      }

      foreach (string username in usernames)
      {
        foreach (string rolename in roleNames)
        {
          if (!IsUserInRole(username, rolename))
          {
            throw new ProviderException(string.Format(CultureInfo.InvariantCulture, "ErrUserIsNotInRole {1} {2}", username, rolename));
          }
        }
      }

      using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
      {
        using (s.BeginTransaction())
        {
          foreach (string username in usernames)
          {
            var usercrit = s.CreateCriteria<User>();
            usercrit.Add(Expression.Eq("ApplicationName", m_applicationName));
            usercrit.Add(Expression.Eq("Username", username));
            var user = usercrit.UniqueResult<User>();
            if (user != null)
            {
              foreach (string rolename in roleNames)
              {
                var rolecrit = s.CreateCriteria<Role>();
                rolecrit.Add(Expression.Eq("ApplicationName", m_applicationName));
                rolecrit.Add(Expression.Eq("Name", rolename));
                var role = rolecrit.UniqueResult<Role>();
                if (role != null)
                {
                  user.RemoveRole(role);
                }
              }
            }
          }
          s.Transaction.Commit();
        }
      }
    }

    /// <summary>
    /// RoleProvider.RoleExists
    /// </summary>
    public override bool RoleExists(string roleName)
    {
      using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
      {
        using (s.BeginTransaction())
        {
          var c = s.CreateCriteria<Role>();
          c.Add(Expression.Eq("Name", roleName));
          c.Add(Expression.Eq("ApplicationName", m_applicationName));
          var role = c.UniqueResult<Role>();
          if (role != null)
          {
            return true;
          }
          else
          {
            return false;
          }
        }
      }
    }
    #endregion
  }
}
