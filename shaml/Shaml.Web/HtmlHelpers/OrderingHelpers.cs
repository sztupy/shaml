using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace Shaml.Web.HtmlHelpers
{
    static public class OrderingHelpers
    {
        static public MvcHtmlString SwitchOrderLink(this HtmlHelper helper, string linkText, string propertyName)
        {
            var RouteToChange = new RouteValueDictionary();
            foreach (var p in helper.ViewContext.RouteData.Values)
            {
                RouteToChange.Add(p.Key, p.Value);
            }
            foreach (string var in helper.ViewContext.HttpContext.Request.QueryString)
            {
                RouteToChange.Add(var, helper.ViewContext.HttpContext.Request.QueryString[var]);
            }
            object actualPropertyObject;
            if (RouteToChange.TryGetValue("OrderBy",out actualPropertyObject)) {
                string actualProperty = actualPropertyObject as string;
                if (actualProperty == propertyName)
                {
                    object actualDescObject;
                    if (RouteToChange.TryGetValue("Desc", out actualDescObject))
                    {
                        RouteToChange["OrderBy"] = propertyName;
                        bool actualDesc;
                        if (Boolean.TryParse(actualDescObject.ToString(), out actualDesc))
                        if (actualDesc == true)
                        {
                            RouteToChange["Desc"] = false;
                            return helper.ActionLink(linkText + " ▲", RouteToChange["Action"].ToString(), RouteToChange);
                        }
                    }
                    RouteToChange["Desc"] = true;
                    return helper.ActionLink(linkText + " ▼", RouteToChange["Action"].ToString(), RouteToChange);
                }
            }
            RouteToChange["Desc"] = false;
            RouteToChange["OrderBy"] = propertyName;
            return helper.ActionLink(linkText,RouteToChange["Action"].ToString(),RouteToChange);
        }
    }
}
