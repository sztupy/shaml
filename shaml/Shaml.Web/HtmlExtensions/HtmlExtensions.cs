using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Linq.Expressions;

namespace Shaml.Web.HtmlExtensions
{
    public static class HtmlExtensions
    {
        public static MvcHtmlString SubmitButton(this HtmlHelper helper, string name)
        {
            return SubmitButton(helper, name, null);
        }

        public static MvcHtmlString SubmitButton(this HtmlHelper helper, string name, string buttonText)
        {
            if (buttonText == null)
            {
                buttonText = "Submit";
            }
            return MvcHtmlString.Create("<input type=\"submit\" name=\""+name+"\" value=\""+buttonText+"\"></input>");
        }

        public static MvcHtmlString ActionLink<TController>(this HtmlHelper helper, Expression<Action<TController>> action, string linkText) where TController : Controller
        {
            return ActionLink<TController>(helper, action, linkText, null);
        }
        public static MvcHtmlString ActionLink<TController>(this HtmlHelper helper, Expression<Action<TController>> action, string linkText, object htmlAttributes) where TController : Controller
        {
            RouteValueDictionary rvd = RouteValueDictionaryBuilder.CreateDictionary<TController>(action);
            return helper.RouteLink(linkText, rvd, new RouteValueDictionary(htmlAttributes));
        }
        public static string Action<TController>(this UrlHelper helper, Expression<Action<TController>> action) where TController : Controller
        {
            RouteValueDictionary rvd = RouteValueDictionaryBuilder.CreateDictionary<TController>(action);
            return helper.Action(rvd["Action"].ToString(), rvd);
        }
        public static MvcForm BeginForm<TController>(this HtmlHelper helper, Expression<Action<TController>> action) where TController : Controller
        {
            RouteValueDictionary rvd = RouteValueDictionaryBuilder.CreateDictionary<TController>(action);
            return helper.BeginForm(rvd);
        }
    }
}
