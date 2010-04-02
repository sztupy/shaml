using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Security.Cryptography;
using System.Web.Hosting;
using System.Web.Configuration;
using System.Web.Security;
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics;
using NHibernate;
using Shaml.Membership.Core;
using Shaml.Data.NHibernate;
using NHibernate.Criterion;

namespace Shaml.Membership
{
    public class NHOpenIDMembershipProvider : MembershipProvider
    {
        private const string s_tableName = "Users";
        private const int s_newPasswordLength = 8;
        private string m_connectionString = string.Empty;

        // Used when determining encryption key values.
        private MachineKeySection m_machineKeyConfig = null;

        /// <summary>
        /// System.Configuration.Provider.ProviderBase.Initialize Method.
        /// </summary>
        public override void Initialize(string name, NameValueCollection config)
        {
            // Initialize values from web.config.
            if (config == null)
                throw new ArgumentNullException("config");

            if (string.IsNullOrEmpty(name))
                name = "NHOpenIDMembershipProvider";

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "NHOpenIDMembershipProvider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            m_applicationName = GetConfigValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);
            m_maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"), CultureInfo.InvariantCulture);
            m_passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"), CultureInfo.InvariantCulture);
            m_minRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"), CultureInfo.InvariantCulture);
            m_minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"), CultureInfo.InvariantCulture);
            m_passwordStrengthRegularExpression = GetConfigValue(config["passwordStrengthRegularExpression"], "");
            m_enablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"), CultureInfo.InvariantCulture);
            m_enablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"), CultureInfo.InvariantCulture);
            m_requiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"), CultureInfo.InvariantCulture);
            m_requiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"), CultureInfo.InvariantCulture);

            // Get password encryption type.
            string pwFormat = GetConfigValue(config["passwordFormat"], "Hashed");
            switch (pwFormat)
            {
                case "Hashed":
                    m_passwordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    m_passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    m_passwordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException();
            }

            // Get connection string.
            //m_connectionString = GetConnectionString(config["connectionStringName"]);

            // Get encryption and decryption key information from the configuration.
            Configuration cfg = WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath);
            m_machineKeyConfig = (MachineKeySection)cfg.GetSection("system.web/machineKey");

            if (!m_passwordFormat.Equals(MembershipPasswordFormat.Clear))
            {
                if (m_machineKeyConfig == null)
                    throw new ProviderException(string.Format(CultureInfo.InvariantCulture, "Config section not found", "system.web/machineKey"));

                if (m_machineKeyConfig.ValidationKey.Contains("AutoGenerate"))
                    throw new ProviderException();
            }
        }

        /// <summary>
        /// System.Web.Security.MembershipProvider properties.
        /// </summary>
        #region System.Web.Security.MembershipProvider properties
        private string m_applicationName = string.Empty;
        private bool m_enablePasswordReset = false;
        private bool m_enablePasswordRetrieval = false;
        private bool m_requiresQuestionAndAnswer = false;
        private bool m_requiresUniqueEmail = false;
        private int m_maxInvalidPasswordAttempts = 0;
        private int m_passwordAttemptWindow = 0;
        private MembershipPasswordFormat m_passwordFormat = MembershipPasswordFormat.Clear;
        private int m_minRequiredNonAlphanumericCharacters = 0;
        private int m_minRequiredPasswordLength = 0;
        private string m_passwordStrengthRegularExpression = string.Empty;

        public override string ApplicationName
        {
            get { return m_applicationName; }
            set { m_applicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return m_enablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return m_enablePasswordRetrieval; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return m_requiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return m_requiresUniqueEmail; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return m_maxInvalidPasswordAttempts; }
        }

        public override int PasswordAttemptWindow
        {
            get { return m_passwordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return m_passwordFormat; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return m_minRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return m_minRequiredPasswordLength; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return m_passwordStrengthRegularExpression; }
        }
        #endregion

        #region NHibernateSession methods

        static public ISession GetNHibernateSession()
        {
            return NHibernateSession.GetDefaultSessionFactory().OpenSession();
        }

        #endregion

        /// <summary>
        /// System.Web.Security.MembershipProvider methods.
        /// </summary>
        #region System.Web.Security.MembershipProvider methods

        /// <summary>
        /// MembershipProvider.ChangePassword
        /// </summary>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (!ValidateUser(username, oldPassword))
                return false;

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException();
            }

            using (ISession s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.Add(Expression.Eq("Username", username));

                try
                {
                    using (s.BeginTransaction())
                    {
                        User user = c.UniqueResult<User>();
                        if (user != null)
                        {
                            user.Password = EncodePassword(newPassword);
                            user.LastPasswordChangedDate = DateTime.Now;
                            s.Save(user);
                            s.Transaction.Commit();
                            return true;
                        }
                        else return false;
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }
            }
        }

        /// <summary>
        /// MembershipProvider.ChangePasswordQuestionAndAnswer
        /// </summary>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            if (!ValidateUser(username, password))
                return false;

            using (ISession s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.Add(Expression.Eq("Username", username));
                try
                {
                    using (s.BeginTransaction())
                    {
                        User user = c.UniqueResult<User>();
                        if (user != null)
                        {
                            user.PasswordQuestion = newPasswordQuestion;
                            user.PasswordAnswer = EncodePassword(newPasswordAnswer);
                            s.Save(user);
                            s.Transaction.Commit();
                            return true;
                        }
                        else return false;
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }
            }
        }

        /// <summary>
        /// MembershipProvider.CreateUser
        /// </summary>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && string.IsNullOrEmpty(email))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }

            if (RequiresUniqueEmail && !string.IsNullOrEmpty(GetUserNameByEmail(email)))
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            if (GetUser(username, false) == null)
            {
                DateTime createDate = DateTime.Now;

                if (providerUserKey != null)
                {
                    throw new NotImplementedException();
                }

                using (var s = GetNHibernateSession())
                {
                    using (s.BeginTransaction())
                    {
                        User u = new User();
                        u.Username = username;
                        u.Password = EncodePassword(password);
                        u.Email = email;
                        u.PasswordQuestion = passwordQuestion ?? "";
                        u.PasswordAnswer = EncodePassword(passwordAnswer) ?? "";
                        u.IsApproved = isApproved;
                        u.CreationDate = createDate;
                        u.LastPasswordChangedDate = createDate;
                        u.LastActivityDate = createDate;
                        u.ApplicationName = m_applicationName;
                        u.IsLockedOut = false;
                        u.LastLockedOutDate = createDate;
                        u.FailedPasswordAnswerAttemptCount = 0;
                        u.FailedPasswordAnswerAttemptWindowStart = createDate;
                        u.FailedPasswordAttemptCount = 0;
                        u.FailedPasswordAttemptWindowStart = createDate;

                        try
                        {
                            object result = s.Save(u);
                            s.Transaction.Commit();
                            if (result != null)
                            {
                                status = MembershipCreateStatus.Success;
                            }
                            else
                            {
                                status = MembershipCreateStatus.UserRejected;
                            }
                        }
                        catch (Exception e)
                        {
                            status = MembershipCreateStatus.ProviderError;
                            Trace.WriteLine(e.ToString());
                            throw new ProviderException();
                        }
                    }
                    return GetUser(username, false);
                }
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }
            return null;
        }

        /// <summary>
        /// MembershipProvider.DeleteUser
        /// </summary>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.Add(Expression.Eq("Username", username));
                try
                {
                    using (s.BeginTransaction())
                    {
                        User user = c.UniqueResult<User>();
                        if (user != null)
                        {
                            s.Delete(user);
                            s.Transaction.Commit();
                            if (deleteAllRelatedData)
                            {
                                // Process commands to delete all data for the user in the database.
                            }
                            return true;
                        }
                        else return false;
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }
            }
        }

        /// <summary>
        /// MembershipProvider.FindUsersByEmail
        /// </summary>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            MembershipUserCollection users = new MembershipUserCollection();

            if (string.IsNullOrEmpty(emailToMatch))
                return users;

            // replace permitted wildcard characters 
            emailToMatch = emailToMatch.Replace('*', '%');
            emailToMatch = emailToMatch.Replace('?', '_');

            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Like("Email", emailToMatch));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.SetProjection(Projections.RowCount());
                totalRecords = c.UniqueResult<int>();

                if (totalRecords <= 0) return users;

                c = s.CreateCriteria<User>();
                c.Add(Expression.Like("Email", emailToMatch));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.SetMaxResults(pageSize);
                c.SetFirstResult(pageSize * pageIndex);
                var result = c.List<User>();

                foreach (User u in result)
                {
                    MembershipUser mu = GetUserFromReader(u);
                    users.Add(mu);
                }
                return users;
            }
        }

        /// <summary>
        /// MembershipProvider.FindUsersByName
        /// </summary>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            MembershipUserCollection users = new MembershipUserCollection();

            // replace permitted wildcard characters 
            usernameToMatch = usernameToMatch.Replace('*', '%');
            usernameToMatch = usernameToMatch.Replace('?', '_');

            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Like("Username", usernameToMatch));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.SetProjection(Projections.RowCount());
                totalRecords = c.UniqueResult<int>();

                if (totalRecords <= 0) return users;

                c = s.CreateCriteria<User>();
                c.Add(Expression.Like("Username", usernameToMatch));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.SetMaxResults(pageSize);
                c.SetFirstResult(pageSize * pageIndex);
                var result = c.List<User>();

                foreach (User u in result)
                {
                    MembershipUser mu = GetUserFromReader(u);
                    users.Add(mu);
                }
                return users;
            }
        }

        /// <summary>
        /// MembershipProvider.GetAllUsers
        /// </summary>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            MembershipUserCollection users = new MembershipUserCollection();
            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.SetProjection(Projections.RowCount());
                totalRecords = c.UniqueResult<int>();

                if (totalRecords <= 0) return users;

                c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.SetMaxResults(pageSize);
                c.SetFirstResult(pageSize * pageIndex);
                var result = c.List<User>();

                foreach (User u in result)
                {
                    MembershipUser mu = GetUserFromReader(u);
                    users.Add(mu);
                }
                return users;
            }
        }

        /// <summary>
        /// MembershipProvider.GetNumberOfUsersOnline
        /// </summary>
        public override int GetNumberOfUsersOnline()
        {
            int numOnline = 0;
            TimeSpan onlineSpan = new TimeSpan(0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);

            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Ge("LastActivityDate", compareTime));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.SetProjection(Projections.RowCount());
                numOnline = c.UniqueResult<int>();

                return numOnline;
            }
        }

        /// <summary>
        /// MembershipProvider.GetPassword
        /// </summary>
        public override string GetPassword(string username, string answer)
        {
            if (!EnablePasswordRetrieval)
            {
                throw new ProviderException();
            }

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new ProviderException();
            }

            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.Add(Expression.Eq("Username", username));
                try
                {
                    User user = c.UniqueResult<User>();
                    if (user != null)
                    {
                        if (user.IsLockedOut)
                            throw new MembershipPasswordException();
                        if (m_requiresQuestionAndAnswer && !CheckPassword(answer, user.PasswordAnswer))
                        {
                            UpdateFailureCount(username, FailureType.PasswordAnswer);
                            throw new MembershipPasswordException();
                        }

                        var password = user.Password;
                        if (m_passwordFormat == MembershipPasswordFormat.Encrypted)
                        {
                            password = UnEncodePassword(user.Password);
                        }
                        return password;
                    }
                    else throw new MembershipPasswordException();
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }
            }
        }

        /// <summary>
        /// MembershipProvider.GetUser
        /// </summary>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            MembershipUser u = null;
            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.Add(Expression.Eq("Username", username));
                try
                {
                    using (s.BeginTransaction())
                    {
                        User user = c.UniqueResult<User>();
                        if (user != null)
                        {
                            u = GetUserFromReader(user);
                            if (userIsOnline)
                            {
                                user.LastActivityDate = DateTime.Now;
                                s.Update(user);
                                s.Transaction.Commit();
                            }
                        }
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }

                return u;
            }
        }

        /// <summary>
        /// MembershipProvider.GetUser
        /// </summary>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            MembershipUser u = null;
            using (var s = GetNHibernateSession())
            {
                try
                {
                    using (s.BeginTransaction())
                    {
                        User user = s.Get<User>(providerUserKey);
                        if (user != null)
                        {
                            u = GetUserFromReader(user);
                            if (userIsOnline)
                            {
                                user.LastActivityDate = DateTime.Now;
                                s.Update(user);
                                s.Transaction.Commit();
                            }
                        }
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }

                return u;
            }
        }

        /// <summary>
        /// MembershipProvider.GetUserNameByEmail
        /// </summary>
        public override string GetUserNameByEmail(string email)
        {
            string username = string.Empty;
            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.Add(Expression.Eq("Email", email));

                try
                {
                    User user = c.UniqueResult<User>();
                    if (user != null)
                    {
                        username = user.Username;
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }
                return username;
            }
        }

        /// <summary>
        /// MembershipProvider.ResetPassword
        /// </summary>
        public override string ResetPassword(string username, string answer)
        {
            if (!m_enablePasswordReset)
            {
                throw new NotSupportedException();
            }

            if (string.IsNullOrEmpty(answer) && m_requiresQuestionAndAnswer)
            {
                UpdateFailureCount(username, FailureType.PasswordAnswer);

                throw new ProviderException();
            }

            string newPassword = System.Web.Security.Membership.GeneratePassword(s_newPasswordLength, m_minRequiredNonAlphanumericCharacters);


            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException();
            }

            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.Add(Expression.Eq("Username", username));
                try
                {
                    using (s.BeginTransaction())
                    {
                        User user = c.UniqueResult<User>();
                        if (user != null)
                        {
                            if (user.IsLockedOut)
                                throw new MembershipPasswordException();

                            if (m_requiresQuestionAndAnswer && !CheckPassword(answer, user.PasswordAnswer))
                            {
                                UpdateFailureCount(username, FailureType.PasswordAnswer);
                                throw new MembershipPasswordException();
                            }

                            c = s.CreateCriteria<User>();
                            c.Add(Expression.Eq("ApplicationName", m_applicationName));
                            c.Add(Expression.Eq("Username", username));
                            c.Add(Expression.Eq("IsLockedOut", false));

                            user = c.UniqueResult<User>();

                            if (user != null)
                            {
                                user.Password = EncodePassword(newPassword);
                                user.LastPasswordChangedDate = DateTime.Now;
                                s.Update(user);
                                s.Transaction.Commit();
                                return user.Password;
                            }
                            else
                                throw new MembershipPasswordException();
                        }
                        else throw new MembershipPasswordException();
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }
            }

        }

        /// <summary>
        /// MembershipProvider.UnlockUser
        /// </summary>
        public override bool UnlockUser(string userName)
        {
            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.Add(Expression.Eq("Username", userName));
                try
                {
                    using (s.BeginTransaction())
                    {
                        User user = c.UniqueResult<User>();
                        if (user != null)
                        {
                            user.IsLockedOut = false;
                            user.LastLockedOutDate = DateTime.Now;
                            s.Update(user);
                            s.Transaction.Commit();
                            return true;

                        }
                        else return false;
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }
            }
        }

        /// <summary>
        /// MembershipProvider.UpdateUser
        /// </summary>
        public override void UpdateUser(MembershipUser user)
        {
            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.Add(Expression.Eq("Username", user.UserName));
                try
                {
                    using (s.BeginTransaction())
                    {
                        User u = c.UniqueResult<User>();
                        if (u != null)
                        {
                            u.Email = user.Email;
                            u.Comment = user.Comment;
                            u.IsApproved = u.IsApproved;
                            s.Update(u);
                            s.Transaction.Commit();
                        }
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }
            }
        }

        /// <summary>
        /// MembershipProvider.ValidateUser
        /// </summary>
        public override bool ValidateUser(string username, string password)
        {
            string dbPassword = string.Empty;
            bool dbIsApproved = false;

            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.Add(Expression.Eq("Username", username));
                c.Add(Expression.Eq("IsLockedOut", false));
                try
                {
                    using (s.BeginTransaction())
                    {
                        User user = c.UniqueResult<User>();
                        if (user != null)
                        {
                            dbPassword = user.Password;
                            dbIsApproved = user.IsApproved;

                            if (CheckPassword(password, dbPassword))
                            {
                                if (dbIsApproved)
                                {
                                    user.LastLoginDate = DateTime.Now;
                                    s.Update(user);
                                    s.Transaction.Commit();
                                    return true;
                                }
                            }
                            return false;
                        }
                        else return false;
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }
            }
        }
        #endregion

        /// <summary>
        /// OpenID support methods
        /// </summary>
        #region OpenID support methods

        public MembershipUser GetUserByOpenID(string id, bool userIsOnline)
        {
            return GetUser(GetUserNameByOpenID(id),userIsOnline);
        }

        public string GetUserNameByOpenID(string id)
        {
            string u = null;

            using (var s = GetNHibernateSession())
            {

                var c = s.CreateQuery("from User u join u.OpenIdAlternatives a where a = :id and u.ApplicationName = :appname");
                c.SetParameter("id", id);
                c.SetParameter("appname", m_applicationName);
                /*
                 * Not possible using Criteria API
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("OpenIdAlternatives.Value", id));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                */

                try
                {
                    User user = c.UniqueResult<User>();
                    if (user != null)
                    {
                        u = user.Username;
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }
                return u;
            }
        }

        public void AddIdToUserName(string UserName, string id, out MembershipCreateStatus status)
        {
            if (String.IsNullOrEmpty(GetUserNameByOpenID(id)))
            {
                using (var s = GetNHibernateSession())
                {
                    var c = s.CreateCriteria<User>();
                    c.Add(Expression.Eq("Username", UserName));
                    c.Add(Expression.Eq("ApplicationName", m_applicationName));

                    try
                    {
                        using (s.BeginTransaction())
                        {
                            User user = c.UniqueResult<User>();
                            if (user != null)
                            {
                                user.OpenIdAlternatives.Add(id);
                                s.Update(user);
                                s.Transaction.Commit();
                                status = MembershipCreateStatus.Success;
                            }
                            else status = MembershipCreateStatus.UserRejected;
                        }
                    }
                    catch (NHibernate.HibernateException e)
                    {
                        Trace.WriteLine(e.ToString());
                        status = MembershipCreateStatus.ProviderError;
                        throw new ProviderException(e.ToString());
                    }
                }
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }
        }

        public void AddIdToUser(MembershipUser user, string id, out MembershipCreateStatus status)
        {
            AddIdToUserName(user.UserName, id, out status);
        }

        public void RemoveIdFromUserName(string UserName, string id, out MembershipCreateStatus status)
        {
            string uname = GetUserNameByOpenID(id);
            if (!String.IsNullOrEmpty(uname) && (uname==UserName))
            {
                using (var s = GetNHibernateSession())
                {
                    var c = s.CreateCriteria<User>();
                    c.Add(Expression.Eq("Username", UserName));
                    c.Add(Expression.Eq("ApplicationName", m_applicationName));
                    try
                    {
                        using (s.BeginTransaction())
                        {
                            User user = c.UniqueResult<User>();
                            if (user != null)
                            {
                                if (user.OpenIdAlternatives.Remove(id))
                                {
                                    s.Update(user);
                                    s.Transaction.Commit();
                                    status = MembershipCreateStatus.Success;
                                }
                                else
                                {
                                    status = MembershipCreateStatus.UserRejected;
                                }

                            }
                            else status = MembershipCreateStatus.UserRejected;
                        }
                    }
                    catch (NHibernate.HibernateException e)
                    {
                        Trace.WriteLine(e.ToString());
                        status = MembershipCreateStatus.ProviderError;
                        throw new ProviderException(e.ToString());
                    }
                    status = MembershipCreateStatus.Success;
                }
            }
            else
            {
                status = MembershipCreateStatus.InvalidPassword;
            }
        }

        public void RemoveIdFromUser(MembershipUser user, string id, out MembershipCreateStatus status)
        {
            RemoveIdFromUserName(user.UserName, id, out status);            
        }

        public List<string> GetOpenIDIdentifiersForUser(MembershipUser user)
        {
            return GetOpenIDIdentifiersForUserName(user.UserName);
        }

        public List<string> GetOpenIDIdentifiersForUserName(string UserName)
        {
            List<string> idlist = new List<string>();
            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("Username", UserName));
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                try
                {
                    User user = c.UniqueResult<User>();
                    if (user != null)
                    {
                        idlist = new List<string>();
                        foreach (var e in user.OpenIdAlternatives)
                        {
                            idlist.Add(e);
                        }
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }
                return idlist;
            }
        }

        #endregion

        #region private methods
        /// <summary>
        /// A helper function to retrieve config values from the configuration file.
        /// </summary>
        /// <param name="configValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        internal static string GetConfigValue(string configValue, string defaultValue)
        {
            if (string.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }

        /// <summary>
        /// A helper function to retrieve the connecion string from the configuration file
        /// </summary>
        /// <param name="connectionStringName">Name of the connection string</param>
        /// <returns></returns>
        internal static string GetConnectionString(string connectionStringName)
        {
            if (string.IsNullOrEmpty(connectionStringName))
                throw new ArgumentException(connectionStringName);

            ConnectionStringSettings ConnectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (ConnectionStringSettings == null || string.IsNullOrEmpty(ConnectionStringSettings.ConnectionString.Trim()))
                throw new ProviderException();

            return ConnectionStringSettings.ConnectionString;
        }

        /// <summary>
        /// A helper function that takes the current row from the NpgsqlDataReader
        /// and hydrates a MembershipUser from the values. Called by the 
        /// MembershipUser.GetUser implementation.
        /// </summary>
        /// <param name="reader">NpgsqlDataReader object</param>
        /// <returns>MembershipUser object</returns>
        private MembershipUser GetUserFromReader(User reader)
        {
            object providerUserKey = reader.Id;
            string username = reader.Username;
            string email = reader.Email;
            string passwordQuestion = reader.PasswordQuestion;
            string comment = reader.Comment;
            bool isApproved = reader.IsApproved;
            bool isLockedOut = reader.IsLockedOut;
            DateTime creationDate = reader.CreationDate;
            DateTime lastLoginDate = reader.LastLoginDate;
            DateTime lastActivityDate = reader.LastActivityDate;
            DateTime lastPasswordChangedDate = reader.LastPasswordChangedDate;
            DateTime lastLockedOutDate = reader.LastLockedOutDate;

            return new MembershipUser(this.Name,
                                                  username,
                                                  providerUserKey,
                                                  email,
                                                  passwordQuestion,
                                                  comment,
                                                  isApproved,
                                                  isLockedOut,
                                                  creationDate,
                                                  lastLoginDate,
                                                  lastActivityDate,
                                                  lastPasswordChangedDate,
                                                  lastLockedOutDate);
        }

        /// <summary>
        /// Compares password values based on the MembershipPasswordFormat.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="dbpassword"></param>
        /// <returns></returns>
        private bool CheckPassword(string password, string dbpassword)
        {
            string pass1 = password;
            string pass2 = dbpassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = UnEncodePassword(dbpassword);
                    break;

                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
                    break;

                default:
                    break;
            }

            if (pass1.Equals(pass2))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Encrypts, Hashes, or leaves the password clear based on the PasswordFormat.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private string EncodePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return password;

            string encodedPassword = password;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;

                case MembershipPasswordFormat.Encrypted:
                    encodedPassword = Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;

                case MembershipPasswordFormat.Hashed:
                    HMACSHA1 hash = new HMACSHA1();
                    hash.Key = HexToByte(m_machineKeyConfig.ValidationKey);
                    encodedPassword = Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;

                default:
                    throw new ProviderException();
            }

            return encodedPassword;
        }

        /// <summary>
        /// Decrypts or leaves the password clear based on the PasswordFormat.
        /// </summary>
        /// <param name="encodedPassword"></param>
        /// <returns></returns>
        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;

                case MembershipPasswordFormat.Encrypted:
                    password = Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;

                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException();

                default:
                    throw new ProviderException();
            }

            return password;
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array. Used to convert encryption
        /// key values from the configuration.
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private static byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);

            return returnBytes;
        }

        /// <summary>
        /// A helper method that performs the checks and updates associated with
        /// password failure tracking.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="failType"></param>
        private void UpdateFailureCount(string username, FailureType failType)
        {
            DateTime windowStart = new DateTime();
            int failureCount = 0;

            using (var s = GetNHibernateSession())
            {
                var c = s.CreateCriteria<User>();
                c.Add(Expression.Eq("ApplicationName", m_applicationName));
                c.Add(Expression.Eq("Username", username));
                try
                {
                    using (s.BeginTransaction())
                    {
                        User user = c.UniqueResult<User>();
                        if (user != null)
                        {
                            if (failType.Equals(FailureType.Password))
                            {
                                failureCount = user.FailedPasswordAttemptCount;
                                windowStart = user.FailedPasswordAttemptWindowStart;
                            }
                            else if (failType.Equals(FailureType.PasswordAnswer))
                            {
                                failureCount = user.FailedPasswordAnswerAttemptCount;
                                windowStart = user.FailedPasswordAnswerAttemptWindowStart;
                            }
                            DateTime windowEnd = windowStart.AddMinutes(m_passwordAttemptWindow);
                            {
                                if (failureCount == 0 || DateTime.Now > windowEnd)
                                {
                                    // First password failure or outside of PasswordAttemptWindow. 
                                    // Start a new password failure count from 1 and a new window starting now.

                                    if (failType.Equals(FailureType.Password))
                                    {
                                        user.FailedPasswordAttemptCount = 1;
                                        user.FailedPasswordAttemptWindowStart = DateTime.Now;
                                        s.Update(user);
                                        s.Transaction.Commit();
                                    }
                                    else if (failType.Equals(FailureType.PasswordAnswer))
                                    {
                                        user.FailedPasswordAnswerAttemptCount = 1;
                                        user.FailedPasswordAnswerAttemptWindowStart = DateTime.Now;
                                        s.Update(user);
                                        s.Transaction.Commit();
                                    }
                                }
                                else
                                {
                                    failureCount++;

                                    if (failureCount >= m_maxInvalidPasswordAttempts)
                                    {
                                        user.IsLockedOut = true;
                                        user.LastLockedOutDate = DateTime.Now;
                                        s.Update(user);
                                        s.Transaction.Commit();
                                    }
                                    else
                                    {
                                        // Password attempts have not exceeded the failure threshold. Update
                                        // the failure counts. Leave the window the same.
                                        if (failType.Equals(FailureType.Password))
                                        {
                                            user.FailedPasswordAttemptCount = failureCount;
                                            s.Update(user);
                                            s.Transaction.Commit();
                                        }
                                        else if (failType.Equals(FailureType.PasswordAnswer))
                                        {
                                            user.FailedPasswordAnswerAttemptCount = failureCount;
                                            s.Update(user);
                                            s.Transaction.Commit();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (NHibernate.HibernateException e)
                {
                    Trace.WriteLine(e.ToString());
                    throw new ProviderException(e.ToString());
                }
            }
        }

        private enum FailureType
        {
            Password,
            PasswordAnswer
        }
        #endregion
    }
}