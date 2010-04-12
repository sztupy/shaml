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
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Web.Profile;
using System.Web.Hosting;
using NHibernate;
using Shaml.Membership.Core;
using Shaml.Data.NHibernate;
using NHibernate.Criterion;
using Shaml.Membership;

namespace Shaml.Membership
{
	public class NHProfileProvider : ProfileProvider
	{
		private const string s_profilesTableName = "Profiles";
		private const string s_profileDataTableName = "ProfileData";
		private const string s_serializationNamespace = "http://schemas.nauck-it.de/PostgreSQLProvider/1.0/";
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
                name = "NHProfileProvider";

			if (string.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
                config.Add("description", "NHProfileProvider");
			}

			// Initialize the abstract base class.
			base.Initialize(name, config);

			m_applicationName = NHOpenIDMembershipProvider.GetConfigValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);

			// Get connection string.
            //m_connectionString = NHOpenIDMembershipProvider.GetConnectionString(config["connectionStringName"]);
		}

		/// <summary>
		/// System.Web.Profile.ProfileProvider properties.
		/// </summary>
		#region System.Web.Security.ProfileProvider properties
		private string m_applicationName = string.Empty;

		public override string ApplicationName
		{
			get { return m_applicationName; }
			set { m_applicationName = value; }
		}
		#endregion

		/// <summary>
		/// System.Web.Profile.ProfileProvider methods.
		/// </summary>
		#region System.Web.Security.ProfileProvider methods

		/// <summary>
		/// ProfileProvider.DeleteInactiveProfiles
		/// </summary>
		public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new NotImplementedException("DeleteInactiveProfiles: The method or operation is not implemented.");
		}

		public override int DeleteProfiles(string[] usernames)
		{
			throw new NotImplementedException("DeleteProfiles1: The method or operation is not implemented.");
		}

		public override int DeleteProfiles(ProfileInfoCollection profiles)
		{
			throw new NotImplementedException("DeleteProfiles2: The method or operation is not implemented.");
		}

		public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException("FindInactiveProfilesByUserName: The method or operation is not implemented.");
		}

		public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException("FindProfilesByUserName: The method or operation is not implemented.");
		}

		public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException("GetAllInactiveProfiles: The method or operation is not implemented.");
		}

		public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException("GetAllProfiles: The method or operation is not implemented.");
		}

		public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new NotImplementedException("GetNumberOfInactiveProfiles: The method or operation is not implemented.");
		}
		#endregion

		/// <summary>
		/// System.Configuration.SettingsProvider methods.
		/// </summary>
		#region System.Web.Security.SettingsProvider methods

		/// <summary>
		/// 
		/// </summary>
		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
		{
			SettingsPropertyValueCollection result = new SettingsPropertyValueCollection();
			string username = (string)context["UserName"];
			bool isAuthenticated = (bool)context["IsAuthenticated"];
			Dictionary<string, object> databaseResult = new Dictionary<string, object>();

            using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
            {
                using (s.BeginTransaction())
                {
                    var c = s.CreateCriteria<Profile>();
                    c.CreateCriteria("User", "u");
                    c.Add(Expression.Eq("u.Username", username));
                    c.Add(Expression.Eq("ApplicationName", m_applicationName));
                    c.Add(Expression.Eq("IsAnonymous", !isAuthenticated));
                    var profiles = c.List<Profile>();
                    foreach (var profile in profiles)
                    {
                        foreach (var pd in profile.Data)
                        {
                            object resultData = null;
                            if (pd.ValueString != null)
                                resultData = pd.ValueString;
                            else if (pd.ValueBinary != null)
                                resultData = pd.ValueBinary;
                            databaseResult.Add(pd.Name,resultData);
                        }
                    }
                }
            }
			foreach (SettingsProperty item in collection)
			{
				if (item.SerializeAs == SettingsSerializeAs.ProviderSpecific)
				{
					if (item.PropertyType.IsPrimitive || item.PropertyType.Equals(typeof(string)))
						item.SerializeAs = SettingsSerializeAs.String;
					else
						item.SerializeAs = SettingsSerializeAs.Xml;
				}

				SettingsPropertyValue itemValue = new SettingsPropertyValue(item);

				if ((databaseResult.ContainsKey(item.Name)) && (databaseResult[item.Name] != null))
				{
					if (item.SerializeAs == SettingsSerializeAs.String)
						itemValue.PropertyValue = SerializationHelper.DeserializeFromBase64<object>((string)databaseResult[item.Name]);

					else if (item.SerializeAs == SettingsSerializeAs.Xml)
						itemValue.PropertyValue = SerializationHelper.DeserializeFromXml<object>((string)databaseResult[item.Name], s_serializationNamespace);

					else if (item.SerializeAs == SettingsSerializeAs.Binary)
						itemValue.PropertyValue = SerializationHelper.DeserializeFromBinary<object>((byte[])databaseResult[item.Name]);
				}
				itemValue.IsDirty = false;				
				result.Add(itemValue);
			}

			UpdateActivityDates(username, isAuthenticated, true);

			return result;
		}

		public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
		{
			string username = (string)context["UserName"];
			bool isAuthenticated = (bool)context["IsAuthenticated"];

			if (string.IsNullOrEmpty(username))
				return;

			if (collection.Count < 1)
				return;

			if (!ProfileExists(username))
				CreateProfileForUser(username, isAuthenticated);

            using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
            {
                using (s.BeginTransaction())
                {
                    foreach (SettingsPropertyValue item in collection)
                    {
                        if (!item.IsDirty)
                            continue;

                        var deletecrit = s.CreateCriteria<Profile>();
                        deletecrit.CreateCriteria("User", "u");
                        deletecrit.Add(Expression.Eq("u.Username", username));
                        deletecrit.Add(Expression.Eq("ApplicationName", m_applicationName));
                        deletecrit.Add(Expression.Eq("IsAnonymous", !isAuthenticated));
                        deletecrit.Add(Expression.Eq("Name", item.Name));
                        var profiles = deletecrit.List<Profile>();
                        foreach (var profile in profiles)
                        {
                            foreach (var pdata in profile.Data)
                            {
                                s.Delete(pdata);
                            }
                            profile.Data.Clear();
                        }
                        s.Transaction.Commit();

                        ProfileData pd = new ProfileData();
                        pd.Name = item.Name; 
                        if (item.Property.SerializeAs == SettingsSerializeAs.String)
                        {
                            pd.ValueString = SerializationHelper.SerializeToBase64(item.PropertyValue);
                            pd.ValueBinary = null;
                        }
                        else if (item.Property.SerializeAs == SettingsSerializeAs.Xml)
                        {
                            item.SerializedValue = SerializationHelper.SerializeToXml<object>(item.PropertyValue, s_serializationNamespace);
                            pd.ValueString = (string)item.SerializedValue;
                            pd.ValueBinary = null;
                        }
                        else if (item.Property.SerializeAs == SettingsSerializeAs.Binary)
                        {
                            item.SerializedValue = SerializationHelper.SerializeToBinary(item.PropertyValue);
                            pd.ValueString = null;
                            pd.ValueBinary = (byte[])item.SerializedValue;
                        }
                        s.Save(pd);
                        foreach (var profile in profiles)
                        {
                            profile.AddProfileData(pd);
                        }
                        s.Transaction.Commit();
                    }
                }
            }
			UpdateActivityDates(username, isAuthenticated, false);
		}
		#endregion

		#region private methods
		/// <summary>
		/// Create a empty user profile
		/// </summary>
		/// <param name="username"></param>
		/// <param name="isAuthenticated"></param>
		private void CreateProfileForUser(string username, bool isAuthenticated)
		{
			if (ProfileExists(username))
				throw new ProviderException(string.Format(CultureInfo.InvariantCulture, "ProfileAlreadyExists {1}", username));

            using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
            {
                using (s.BeginTransaction())
                {
                    var c = s.CreateCriteria<User>();
                    c.Add(Expression.Eq("Username", username));
                    c.Add(Expression.Eq("ApplicationName", m_applicationName));
                    var user = c.UniqueResult<User>();
                    if (user != null)
                    {
                        Profile p = new Profile();
                        p.ApplicationName = m_applicationName;
                        p.IsAnonymous = !isAuthenticated;
                        p.LastActivityDate = DateTime.Now;
                        p.LastUpdatedDate = DateTime.Now;
                        s.Save(p);
                        user.SetProfile(p);
                    }
                    s.Transaction.Commit();
                }
            }
		}


		private bool ProfileExists(string username)
		{
            using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
            {
                using (s.BeginTransaction())
                {
                    var c = s.CreateCriteria<User>();
                    c.Add(Expression.Eq("Username", username));
                    c.Add(Expression.Eq("ApplicationName", m_applicationName));
                    var user = c.UniqueResult<User>();
                    if (user != null)
                    {
                        if (user.UserProfile != null)
                        {
                            return true;
                        }
                    }
                }
            }

			return false;
		}

		/// <summary>
		/// Updates the LastActivityDate and LastUpdatedDate values when profile properties are accessed by the
		/// GetPropertyValues and SetPropertyValues methods.
		/// Passing true as the activityOnly parameter will update only the LastActivityDate.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="isAuthenticated"></param>
		/// <param name="activityOnly"></param>
		private void UpdateActivityDates(string username, bool isAuthenticated, bool activityOnly)
		{
            using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
            {
                using (s.BeginTransaction())
                {
                    var c = s.CreateCriteria<Profile>();
                    c.CreateCriteria("User", "u");
                    c.Add(Expression.Eq("u.Username", username));
                    c.Add(Expression.Eq("ApplicationName", m_applicationName));
                    c.Add(Expression.Eq("IsAnonymous", !isAuthenticated));
                    var profiles = c.List<Profile>();
                    foreach (var profile in profiles)
                    {
                        profile.LastActivityDate = DateTime.Now;
                        if (!activityOnly)
                        {
                            profile.LastUpdatedDate = DateTime.Now;
                        }
                        s.Update(profile);
                    }
                    s.Transaction.Commit();
                }
            }
		}
		#endregion
	}
}
