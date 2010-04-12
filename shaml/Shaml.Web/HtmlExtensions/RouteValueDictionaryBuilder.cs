using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;

namespace Shaml.Web.HtmlExtensions
{
    static public class RouteValueDictionaryBuilder
    {
        // works for simple controller method callings only
        public static RouteValueDictionary CreateDictionary<TController>(Expression<Action<TController>> action) where TController : Controller
        {
            if ((action == null) || (!(action.Body is MethodCallExpression)))
            {
                throw new ArgumentNullException("action");
            }
            var mc = action.Body as MethodCallExpression;
            var controllername = typeof(TController).Name.Replace("Controller","");
            var actionname = mc.Method.Name;

            var rvd = new RouteValueDictionary();
            rvd.Add("Controller", controllername);
            rvd.Add("Action", actionname);

            ParameterInfo[] parameters = mc.Method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var arg = mc.Arguments[i];
                var name = parameters[i].Name;
                ConstantExpression value;
                if ((value = arg as ConstantExpression) != null)
                {
                    rvd.Add(parameters[i].Name, value.Value);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return rvd;
        }
    }
}
