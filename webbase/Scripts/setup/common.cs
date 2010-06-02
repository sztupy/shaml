LoadAssembly("bin/FluentNHibernate.dll");
LoadAssembly("bin/Iesi.Collections.dll");
LoadAssembly("bin/LinFu.Core.dll");
LoadAssembly("bin/LinFu.DynamicProxy.dll");
LoadAssembly("bin/log4net.dll");
LoadAssembly("bin/Microsoft.Practices.ServiceLocation.dll");
LoadAssembly("bin/Mono.Security.dll");
LoadAssembly("bin/Newtonsoft.Json.dll");
LoadAssembly("bin/NHaml.Core.dll");
LoadAssembly("bin/NHaml.Web.Mvc.dll");
LoadAssembly("bin/NHibernate.dll");
LoadAssembly("bin/NHibernate.ByteCode.LinFu.dll");
LoadAssembly("bin/NHibernate.Validator.dll");
LoadAssembly("bin/Npgsql.dll");
LoadAssembly("bin/Shaml.Core.dll");
LoadAssembly("bin/Shaml.Core.Validator.dll");
LoadAssembly("bin/Shaml.Data.dll");
LoadAssembly("bin/Shaml.Membership.dll");
LoadAssembly("bin/Shaml.Testing.dll");
LoadAssembly("bin/Shaml.Tests.dll");
LoadAssembly("bin/Shaml.Web.dll");
LoadAssembly("bin/WebBase.dll");
LoadAssembly("bin/WebBase.ApplicationServices.dll");
LoadAssembly("bin/WebBase.Config.dll");
LoadAssembly("bin/WebBase.Controllers.dll");
LoadAssembly("bin/WebBase.Core.dll");
LoadAssembly("bin/WebBase.Data.dll");
LoadAssembly("bin/WebBase.Tests.dll");

using System;
using System.IO;
using System.Collections.Generic;
using WebBase.Data.Mapping;
using WebBase.Core;
using Shaml.Membership.Core;
using Shaml.Data.NHibernate;
using Shaml.Testing.NHibernate;
using NHibernate;
using NHibernate.Metadata;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Criterion;

Configuration configuration;
string[] mappingAssemblies = new string[1];
mappingAssemblies[0] = "bin/WebBase.Data.dll";
configuration = NHibernateSession.Init(
  new SimpleSessionStorage(), mappingAssemblies,
  new AutoPersistenceModelGenerator().Generate(),
  "Config/NHibernate.config");
WebBase.Config.ComponentRegistrar.InitializeServiceLocator();
var s = NHibernateSession.GetDefaultSessionFactory().OpenSession();

