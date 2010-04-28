using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Reflection;
using System.ComponentModel;

namespace Shaml.Web.HtmlHelpers
{
    public class EnumDisplayNameAttribute : Attribute
    {
        public virtual string DisplayName { get; set; }

        public EnumDisplayNameAttribute()
        {
            DisplayName = "";
        }

        public EnumDisplayNameAttribute(string DisplayName)
        {
            this.DisplayName = DisplayName;
        }
    }

    static public class EnumHelper
    {
        static public IEnumerable<SelectListItem> SelectList<T>()
        {
            return SelectList<T>(false);
        }

        static public IEnumerable<SelectListItem> SelectList<T>(bool OnlyIfAttribExists) {
            Type dataType = Enum.GetUnderlyingType(typeof(T));
            foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public))
            {
                bool hasAttrib = false;
                string Name = field.Name;
                string Value = field.Name;
                foreach (Attribute attrib in field.GetCustomAttributes(true))
                {
                    if (attrib is EnumDisplayNameAttribute)
                    {
                        Name = (attrib as EnumDisplayNameAttribute).DisplayName;
                        hasAttrib = true;
                    }
                }
                if (hasAttrib || !OnlyIfAttribExists)
                {
                    yield return new SelectListItem()
                    {
                        Value = Value,
                        Text = Name
                    };
                }
            }
        }
    }
}
