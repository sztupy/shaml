using System;
using System.Reflection;

namespace Shaml.Core
{
    /// <summary>
    /// Provides an Attribute to set the display name for the items in the enum
    /// </summary>
    public class EnumDisplayNameAttribute : Attribute
    {
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// The display name should be the same as the value itself
        /// </summary>
        public EnumDisplayNameAttribute()
        {
            DisplayName = null;
        }

        public EnumDisplayNameAttribute(string DisplayName)
        {
            this.DisplayName = DisplayName;
        }
    }

    public static class EnumDisplayNameHelper
    {
        public static string DisplayName(this Enum en)
        {
            // Check for the EnumDisplayName attribute
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(EnumDisplayNameAttribute),
                                                                false);
                if (attrs != null && attrs.Length > 0)
                    return ((EnumDisplayNameAttribute)attrs[0]).DisplayName;

            }
            // Fall back if no DisplayNameAttribute was found
            return en.ToString();
        }
    }

    public class Enums
    {
        /// <summary>
        /// Provides an NHibernate.LockMode facade so as to avoid a direct dependency on the NHibernate DLL.
        /// Further information concerning lockmodes may be found at:
        /// http://www.hibernate.org/hib_docs/nhibernate/html_single/#transactions-locking
        /// </summary>
        public enum LockMode
        {
            None,
            Read,
            Upgrade,
            UpgradeNoWait,
            Write
        }
    }
}
