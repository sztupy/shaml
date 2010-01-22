using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using log4net;
using System.Web.Routing;
using System.Web;

namespace Shaml.Web
{
    public class GenericLoggerAttribute : FilterAttribute, IActionFilter, IResultFilter, IExceptionFilter
    {
        public log4net.ILog log;

        public GenericLoggerAttribute()
        {
            log = log4net.LogManager.GetLogger(typeof(System.Web.Mvc.Controller));
        }

        public GenericLoggerAttribute(ILog logger)
        {
            log = logger;
        }

        private string RouteDataToString(RouteData route)
        {
            return " Controller: " + route.Values["controller"] +
                   " Action: " + route.Values["action"] +
                   " Route: " + route.Values.Aggregate("", (seed,obj) => seed + obj.Key + "=" + obj.Value + ";");
        }

        private string RequestDataToString(HttpRequestBase request)
        {
            return " FilePath: " + request.FilePath +
                   " Method: " + request.HttpMethod +
                   " IP: " + request.UserHostAddress;
        }

        #region IActionFilter Members

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            log.Info("Action Executed:" + RequestDataToString(filterContext.HttpContext.Request) + RouteDataToString(filterContext.RouteData));
                                        
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            log.Info("Action Executing:" + RequestDataToString(filterContext.HttpContext.Request) + RouteDataToString(filterContext.RouteData));
        }

        #endregion

        #region IResultFilter Members

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            log.Info("Result Executed:" + RequestDataToString(filterContext.HttpContext.Request) + RouteDataToString(filterContext.RouteData));
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            log.Info("Result Executing:" + RequestDataToString(filterContext.HttpContext.Request) + RouteDataToString(filterContext.RouteData));
        }

        #endregion

        #region IExceptionFilter Members

        public void OnException(ExceptionContext filterContext)
        {
            log.Error("Exception Occured: " + filterContext.Exception.Message);
        }

        #endregion
    }
}
