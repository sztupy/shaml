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
using WebBase.Core.Mapping;

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
            nhaml.PartialViewLocationFormats = new[] {
                "~/App/Views/{1}/_{0}.haml",
                "~/App/Views/Shared/_{0}.haml"
            };

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(nhaml);

            WebBase.AppServices.RouteRegistrar.RegisterRoutesTo(RouteTable.Routes);
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

            // Only allow the NHibernate session to be initialized once
            if (!wasNHibernateInitialized)
            {
                lock (lockObject)
                {
                    if (!wasNHibernateInitialized)
                    {

                        var cfg = NHibernateSession.Init(new WebSessionStorage(this),
                            new string[] { Server.MapPath("~/bin/WebBase.dll") },
                            new AutoPersistenceModelGenerator().Generate(),
                            Server.MapPath("~/NHibernate.config"));

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