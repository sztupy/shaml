using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NHaml.Web;
using NHaml.Web.Mvc;
using System.Reflection;
using Shaml.Data.NHibernate;
using Shaml.Web.NHibernate;
using WebBase.Data.Mapping;
using Shaml.Web.ModelBinder;
using LinFu.IoC;
using CommonServiceLocator.LinFuAdapter;
using Microsoft.Practices.ServiceLocation;
using WebBase.Config;
using NHibernate.Dialect;
using NHibernate.Tool.hbm2ddl;
using Shaml.Web.Areas;

namespace WebBase
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode,
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Due to issues on IIS7, the NHibernate initialization cannot reside in Init() but
        /// must only be called once.  Consequently, we invoke a thread-safe singleton class to
        /// ensure it's only initialized once.
        /// </summary>
        protected void Application_BeginRequest(object sender, EventArgs e) {
            NHibernateInitializer.Instance().InitializeNHibernateOnce(
                () => InitializeNHibernateSession());
        }

        /// <summary>
        /// If you need to communicate to multiple databases, you'd add a line to this method to
        /// initialize the other database as well.
        /// </summary>
        private void InitializeNHibernateSession() {
          var cfg = NHibernateSession.Init(new WebSessionStorage(this),
              new string[] { Server.MapPath("~/bin/WebBase.Data.dll") },
              new AutoPersistenceModelGenerator().Generate(),
              Server.MapPath("~/Config/NHibernate.config"));
        }

        private WebSessionStorage webSessionStorage;


        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();

            var nhaml = new NHamlMvcViewEngine();
            nhaml.MasterLocationFormats = new[] {
                "~/App/Views/Shared/{0}.haml",
                "~/App/Views/Shared/Application.haml",
            };
            nhaml.ViewLocationFormats = new[] {
                "~/App/Views/{1}/{0}.haml",
                "~/App/Views/Shared/{0}.haml"
            };
            nhaml.PartialViewLocationFormats = nhaml.ViewLocationFormats;

            var area = new AreaViewEngine();

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(nhaml);
            ViewEngines.Engines.Add(area);

            ModelBinders.Binders.DefaultBinder = new SharpModelBinder();

            ComponentRegistrar.InitializeServiceLocator();

      			AreaRegistration.RegisterAllAreas();
            RouteRegistrar.RegisterRoutesTo(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Useful for debugging
            Exception ex = Server.GetLastError();
            ReflectionTypeLoadException reflectionTypeLoadException = ex as ReflectionTypeLoadException;
        }

        public override void Init()
        {
            base.Init();
            // The WebSessionStorage must be created during the Init() to tie in HttpApplication events
            webSessionStorage = new WebSessionStorage(this);
        }

    }
}
