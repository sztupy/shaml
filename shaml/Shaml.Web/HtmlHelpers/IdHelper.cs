using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Reflection;
using System.ComponentModel;
using Shaml.Core;
using System.Linq.Expressions;

namespace Shaml.Web.HtmlHelpers
{
    static public class IdHelper
    {
        static public MvcHtmlString IdHelperFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return MvcHtmlString.Create(htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(ExpressionHelper.GetExpressionText(expression)));
        }

        static public MvcHtmlString NameHelperFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return MvcHtmlString.Create(ExpressionHelper.GetExpressionText(expression));
        }
    }
}
