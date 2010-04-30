using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Reflection;
using System.ComponentModel;
using Shaml.Core;

namespace Shaml.Web.HtmlHelpers
{
    static public class EnumHelper
    {
        /// <summary>
        /// Creates a SelectListItem Enumerable from the data avialable from the <see cref="EnumDisplayNameAttribute"/>
        /// </summary>
        /// <typeparam name="T">The enum type to get the data from</typeparam>
        /// <returns>The list of selectitems</returns>
        static public IEnumerable<SelectListItem> SelectList<T>()
        {
            return SelectList<T>(false);
        }

        /// <summary>
        /// Creates a SelectListItem Enumerable from the data avialable from the <see cref="EnumDisplayNameAttribute"/>
        /// </summary>
        /// <typeparam name="T">The enum type to get the data from</typeparam>
        /// <param name="OnlyIfAttribExists">Returns a list item only if the EnumDisplayNameAttribute exist</param>
        /// <returns>The list of selectitems</returns>
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
