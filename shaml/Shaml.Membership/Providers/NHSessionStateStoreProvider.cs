//
// $Id$
//
// Copyright � 2007 - 2008 Nauck IT KG		http://www.nauck-it.de
//
// Authors:
//	Daniel Nauck		<d.nauck(at)nauck-it.de>
//	Christ Akkermans	<c.akkermans(at)vereyon.nl>
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
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.SessionState;
using Shaml.Membership.Core;
using NHibernate.Criterion;

namespace Shaml.Membership
{
	public class NHSessionStateStoreProvider : SessionStateStoreProviderBase
	{
		private const string s_tableName = "Sessions";
		private System.Timers.Timer m_expiredSessionDeletionTimer;
		private string m_connectionString = string.Empty;
		private string m_applicationName = string.Empty;
		private SessionStateSection m_config = null;
		private bool m_enableExpireCallback = false;
		private SessionStateItemExpireCallback m_expireCallback = null;

		/// <summary>
		/// System.Configuration.Provider.ProviderBase.Initialize Method
		/// </summary>
		public override void Initialize(string name, NameValueCollection config)
		{
			// Initialize values from web.config.
			if (config == null)
				throw new ArgumentNullException("config");

			if (string.IsNullOrEmpty(name))
				name = "NHSessionStateStoreProvider";

			if (string.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
                config.Add("description", "NHSessionStateStoreProvider");
			}

			// Initialize the abstract base class.
			base.Initialize(name, config);

			m_applicationName = NHOpenIDMembershipProvider.GetConfigValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);

			// Get connection string.
			//m_connectionString = PgMembershipProvider.GetConnectionString(config["connectionStringName"]);

			// Get <sessionState> configuration element.
			m_config = (SessionStateSection)WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath).GetSection("system.web/sessionState");

			// Should automatic session garbage collection be turned on?
			bool enableExpiredSessionAutoDeletion = Convert.ToBoolean(NHOpenIDMembershipProvider.GetConfigValue(config["enableExpiredSessionAutoDeletion"], "false"), CultureInfo.InvariantCulture);
			
			if (!enableExpiredSessionAutoDeletion)
				return;

            m_enableExpireCallback = Convert.ToBoolean(NHOpenIDMembershipProvider.GetConfigValue(config["enableSessionExpireCallback"], "false"), CultureInfo.InvariantCulture);

			// Load session garbage collection configuration and setup garbage collection interval timer
            double expiredSessionAutoDeletionInterval = Convert.ToDouble(NHOpenIDMembershipProvider.GetConfigValue(config["expiredSessionAutoDeletionInterval"], "1800000"), CultureInfo.InvariantCulture); //default: 30 minutes

			m_expiredSessionDeletionTimer = new System.Timers.Timer(expiredSessionAutoDeletionInterval);
			m_expiredSessionDeletionTimer.Elapsed += new System.Timers.ElapsedEventHandler(ExpiredSessionDeletionTimer_Elapsed);
			m_expiredSessionDeletionTimer.Enabled = true;
			m_expiredSessionDeletionTimer.AutoReset = true;
		}

		/// <summary>
		/// SessionStateStoreProviderBase members
		/// </summary>
		#region SessionStateStoreProviderBase members

		public override void Dispose()
		{
			if (m_expiredSessionDeletionTimer == null)
				return;

			// cleanup timer
			m_expiredSessionDeletionTimer.Stop();
			m_expiredSessionDeletionTimer.Dispose();
			m_expiredSessionDeletionTimer = null;
		}

		/// <summary>
		/// SessionStateProviderBase.InitializeRequest
		/// </summary>
		public override void InitializeRequest(HttpContext context)
		{
		}

		/// <summary>
		/// SessionStateProviderBase.EndRequest
		/// </summary>
		public override void EndRequest(HttpContext context)
		{
		}

		/// <summary>
		/// SessionStateProviderBase.CreateNewStoreData
		/// </summary>
		public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
		{
			return new SessionStateStoreData(new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout);
		}

		/// <summary>
		/// SessionStateProviderBase.CreateUninitializedItem
		/// </summary>
		public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
		{
            using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
            {
                using (s.BeginTransaction())
                {
                    Session session = new Session();
                    session.SessionId = id;
                    session.ApplicationName = m_applicationName;
                    session.Created = DateTime.Now;
                    session.Expires = DateTime.Now.AddMinutes((Double)timeout);
                    session.Timeout = timeout;
                    session.Locked = false;
                    session.LockId = 0;
                    session.LockDate = DateTime.Now;
                    session.Data = string.Empty;
                    session.Flags = 1;
                    s.Save(session);
                    s.Transaction.Commit();
                }
            }
		}

		/// <summary>
		/// SessionStateProviderBase.GetItem
		/// </summary>
		public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			return GetSessionStoreItem(false, context, id, out locked, out lockAge, out lockId, out actions);
		}

		/// <summary>
		/// SessionStateProviderBase.GetItemExclusive
		/// </summary>
		public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
		{
			return GetSessionStoreItem(true, context, id, out locked, out lockAge, out lockId, out actions);
		}

		/// <summary>
		/// SessionStateProviderBase.ReleaseItemExclusive
		/// </summary>
		public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
		{
            using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
            {
                var c = s.CreateCriteria<Session>();
                c.Add(Expression.Eq("SessionId", id));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.Add(Expression.Eq("LockId", lockId));
                try
                {
                    using (s.BeginTransaction())
                    {
                        var session = c.UniqueResult<Session>();
                        if (session != null)
                        {
                            session.Expires = DateTime.Now.Add(m_config.Timeout);
                            session.Locked = false;
                            s.Update(session);
                            s.Transaction.Commit();
                        }
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    throw new ProviderException(e.ToString());
                }
            }
		}

		/// <summary>
		/// SessionStateProviderBase.RemoveItem
		/// </summary>
		public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
		{
            using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
            {
                var c = s.CreateCriteria<Session>();
                c.Add(Expression.Eq("SessionId", id));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.Add(Expression.Eq("LockId", lockId));
                try
                {
                    using (s.BeginTransaction())
                    {
                        var session = c.UniqueResult<Session>();
                        if (session != null)
                        {
                            s.Delete(session);
                            s.Transaction.Commit();
                        }
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    throw new ProviderException(e.ToString());
                }
            }
		}

		/// <summary>
		/// SessionStateProviderBase.ResetItemTimeout
		/// </summary>
		public override void ResetItemTimeout(HttpContext context, string id)
		{
            using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
            {
                var c = s.CreateCriteria<Session>();
                c.Add(Expression.Eq("SessionId", id));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                try
                {
                    using (s.BeginTransaction())
                    {
                        var session = c.UniqueResult<Session>();
                        if (session != null)
                        {
                            session.Expires = DateTime.Now.Add(m_config.Timeout);
                            s.Update(session);
                            s.Transaction.Commit();
                        }
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    throw new ProviderException(e.ToString());
                }
            }
		}

		/// <summary>
		/// SessionStateProviderBase.SetAndReleaseItemExclusive
		/// </summary>
		public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
		{
			// Serialize the SessionStateItemCollection as a string
			string serializedItems = Serialize((SessionStateItemCollection)item.Items);
            using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
            {
                var c = s.CreateCriteria<Session>();
                c.Add(Expression.Eq("SessionId", id));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                try
                {
                    var sessions = c.List<Session>();
                    if (newItem)
                    {
                        using (s.BeginTransaction())
                        {
                            foreach (var session in sessions)
                            {
                                s.Delete(session);
                            }
                            var sess = new Session();
                            sess.SessionId = id;
                            sess.ApplicationName = m_applicationName;
                            sess.Created = DateTime.Now;
                            sess.Expires = DateTime.Now.AddMinutes((Double)item.Timeout);
                            sess.Timeout = item.Timeout;
                            sess.Locked = false;
                            sess.LockId = 0;
                            sess.LockDate = DateTime.Now;
                            sess.Data = serializedItems;
                            sess.Flags = 0;
                            s.Save(sess);
                            s.Transaction.Commit();
                        }
                    }
                    else
                    {
                        using (s.BeginTransaction())
                        {
                            foreach (var session in sessions)
                            {
                                if (session.LockId == (int)lockId)
                                {
                                    session.Expires = DateTime.Now.AddMinutes((Double)item.Timeout);
                                    session.Locked = false;
                                    session.Data = serializedItems;
                                    s.Update(session);
                                }
                            }
                            s.Transaction.Commit();
                        }
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    throw new ProviderException(e.ToString());
                }
            }
		}

		/// <summary>
		/// SessionStateProviderBase.SetItemExpireCallback
		/// </summary>
		public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
		{
			// Accept and store callback if session expire callback is enabled. If not, return false in order to inform SessionStateModule
			// the session expire callback is not supported.
			if (!m_enableExpireCallback)
				return false;

			m_expireCallback = expireCallback;
			return true;
		}

		#endregion

		#region private methods

		/// <summary>
		/// Retrieves the session data from the data source.
		/// </summary>
		/// <param name="lockRecord">If true GetSessionStoreItem locks the record and sets a new LockId and LockDate.</param>	
		private SessionStateStoreData GetSessionStoreItem(bool lockRecord, HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
		{
			SessionStateStoreData result = null;
			lockAge = TimeSpan.Zero;
			lockId = null;
			locked = false;
			actionFlags = 0;
			DateTime expires = DateTime.MinValue;
			int timeout = 0;
			string serializedItems = null;

            using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
            {
                var c = s.CreateCriteria<Session>();
                c.Add(Expression.Eq("SessionId", id));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.SetLockMode(NHibernate.LockMode.Upgrade);
                try
                {
                    using (s.BeginTransaction())
                    {
                        var sessions = c.List<Session>();
                        foreach (var session in sessions)
                        {
                            expires = session.Expires;
                            timeout = session.Timeout;
                            locked = session.Locked;
                            lockId = session.LockId;
                            lockAge = DateTime.Now.Subtract(session.LockDate);

                            if (session.Data != null)
                            {
                                serializedItems = session.Data;
                            }

                            actionFlags = (SessionStateActions)session.Flags;
                        }
                        // If record was not found, is expired or is locked, return.
                        if (expires < DateTime.Now || locked)
                            return result;

                        // If the actionFlags parameter is not InitializeItem, deserialize the stored SessionStateItemCollection
                        if (actionFlags == SessionStateActions.InitializeItem)
                            result = CreateNewStoreData(context, Convert.ToInt32(m_config.Timeout.TotalMinutes));
                        else
                            result = new SessionStateStoreData(Deserialize(serializedItems), SessionStateUtility.GetSessionStaticObjects(context), Convert.ToInt32(m_config.Timeout.TotalMinutes));

                        if (lockRecord)
                        {
                            lockId = (int)lockId + 1;

                            foreach (var session in sessions)
                            {
                                session.Locked = true;
                                session.LockId = (int)lockId;
                                session.LockDate = DateTime.Now;
                                session.Flags = 0;
                                s.Save(session);
                                s.Transaction.Commit();
                            }
                        }
                    }
                    return result;
                }
                catch (NHibernate.HibernateException e)
                {
                    throw new ProviderException(e.ToString());
                }
            }
		}

		/// <summary>
		/// Convert a SessionStateItemCollection into a Base64 string
		/// </summary>
		private static string Serialize(SessionStateItemCollection items)
		{
			if (items == null || items.Count < 1)
				return string.Empty;

			using (MemoryStream mStream = new MemoryStream())
			{
				using (BinaryWriter bWriter = new BinaryWriter(mStream))
				{
					items.Serialize(bWriter);
					bWriter.Close();
				}

				return Convert.ToBase64String(mStream.ToArray());
			}
		}

		/// <summary>
		/// Convert a Base64 string into a SessionStateItemCollection
		/// </summary>
		/// <param name="serializedItems"></param>
		/// <returns></returns>
		private static SessionStateItemCollection Deserialize(string serializedItems)
		{
			SessionStateItemCollection sessionItems = new SessionStateItemCollection();

			if (string.IsNullOrEmpty(serializedItems))
				return sessionItems;

			using (MemoryStream mStream = new MemoryStream(Convert.FromBase64String(serializedItems)))
			{
				using (BinaryReader bReader = new BinaryReader(mStream))
				{
					sessionItems = SessionStateItemCollection.Deserialize(bReader);
					bReader.Close();
				}
			}

			return sessionItems;
		}

		/// <summary>
		/// The ExpiredSessionDeletionTimer_Elapsed performs automatic session garbage collection by removing expired sessions from
		/// the database.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void ExpiredSessionDeletionTimer_Elapsed(object source, System.Timers.ElapsedEventArgs e)
		{
			//
			// * Determine mode of session garbage collection. If the session expire callback is disabled
			// * one may simple delete all expired session from the session table. If however the session expire callback
			// * is enabled, we need to load the session data for every expired session and invoke the expire callback
			// * for each of these sessions prior to deletion.
			// * Also check if an expire call back was actually defined. If m_expireCallback is null we also don't have to take
			// * the more expensive path where every session is enumerated while there's no real need to do so.
			//

			if (m_enableExpireCallback && m_expireCallback != null)
				InvokeExpireCallbackAndDeleteSession();

			else
				DeleteExpiredSessionsFromDatabase();
		}

		/// <summary>
		/// Load the session data for every expired session and invoke the expire callback
		/// for each of these sessions prior to deletion.
		/// </summary>
		private void InvokeExpireCallbackAndDeleteSession()
		{
            using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
            {
                var c = s.CreateCriteria<Session>();
                c.Add(Expression.Lt("Expires", DateTime.Now));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                try
                {
                    using (s.BeginTransaction())
                    {
                        var sessions = c.List<Session>();
                        foreach (var session in sessions)
                        {
                            SessionStateStoreData d = new SessionStateStoreData(Deserialize(session.Data), new HttpStaticObjectsCollection(), Convert.ToInt32(m_config.Timeout.TotalMinutes));
                            m_expireCallback.Invoke(session.SessionId, d);
                            s.Delete(session);
                            s.Transaction.Commit();
                        }
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    throw new ProviderException(e.ToString());
                }
            }
		}

		/// <summary>
		/// Delete all expired session from the session table.
		/// </summary>
		private void DeleteExpiredSessionsFromDatabase()
		{
            using (var s = NHOpenIDMembershipProvider.GetNHibernateSession())
            {
                var c = s.CreateCriteria<Session>();
                c.Add(Expression.Lt("Expires", DateTime.Now));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                try
                {
                    using (s.BeginTransaction())
                    {
                        var sessions = c.List<Session>();
                        foreach (var session in sessions)
                        {
                            s.Delete(session);
                            s.Transaction.Commit();
                        }
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    throw new ProviderException(e.ToString());
                }
            }
		}

		#endregion
	}
}
