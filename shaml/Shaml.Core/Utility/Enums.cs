using System;

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
