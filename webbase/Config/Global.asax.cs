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

namespace WebBase
{
    public class MvcApplication : System.Web.HttpApplication
    {
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

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(nhaml);

            ModelBinders.Binders.DefaultBinder = new SharpModelBinder();
            InitializeServiceLocator();

            RouteRegistrar.RegisterRoutesTo(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Useful for debugging
            Exception ex = Server.GetLastError();
            ReflectionTypeLoadException reflectionTypeLoadException = ex as ReflectionTypeLoadException;
        }

        protected virtual void InitializeServiceLocator()
        {
            ServiceContainer container = new ServiceContainer();
            //ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(container));
            //container.RegisterControllers(typeof(HomeController).Assembly);

            ComponentRegistrar.AddComponentsTo(container);

            ServiceLocator.SetLocatorProvider(() => new LinFuServiceLocator(container));
        }

        public override void Init()
        {
            base.Init();

            // Only allow the NHibernate session to be initialized once
            if (!wasNHibernateInitialized)
            {
                lock (lockObject)
                {
                    if (!wasNHibernateInitialized)
                    {

                        var cfg = NHibernateSession.Init(new WebSessionStorage(this),
                            new string[] { Server.MapPath("~/bin/WebBase.Data.dll") },
                            new AutoPersistenceModelGenerator().Generate(),
                            Server.MapPath("~/Config/NHibernate.config"));

                        //apm.WriteMappingsTo(@"e:\Programs\Blog\db");
                        var script = cfg.GenerateSchemaCreationScript(new NHibernate.Dialect.PostgreSQL82Dialect());

                        using (var tw = new System.IO.StreamWriter(System.IO.Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath,System.IO.Path.Combine("db","Auto_Mapping_Schema.sql"))))
                        {
                            foreach (string s in script)
                            {
                                tw.WriteLine(s);
                            }
                        }

                        wasNHibernateInitialized = true;
                    }
                }
            }
        }

        private static bool wasNHibernateInitialized = false;

        /// <summary>
        /// Private, static object used only for synchronization
        /// </summary>
        private static object lockObject = new object();

    }
}