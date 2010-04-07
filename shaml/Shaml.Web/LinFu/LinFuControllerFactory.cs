using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using LinFu.IoC;

namespace Shaml.Web.LinFu
{
  // based on http://geekswithblogs.net/thomasweller/archive/2009/12/07/integrating-a-linfu-ioc-container-with-your-asp.net-mvc-application.aspx
  [CLSCompliant(false)]
  public class LinFuControllerFactory : DefaultControllerFactory
  {
    protected ServiceContainer Container { get; private set; }

    public LinFuControllerFactory(ServiceContainer serviceContainer)
    {
      if (serviceContainer == null)
      {
        throw new ArgumentNullException("serviceContainer");
      }
      this.Container = serviceContainer;
    }

    protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
    {
      if (controllerType == null)
      {
        throw new HttpException(404, string.Format(
            "The controller for path '{0}' could not be found or it does not implement IController.",
            requestContext.HttpContext.Request.Path));
      }

      return (IController)controllerType.AutoCreateFrom(this.Container);
    }

  } // class LinFuControllerFactory
}
