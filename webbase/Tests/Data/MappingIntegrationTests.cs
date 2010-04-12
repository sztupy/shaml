using System.Collections.Generic;
using NHibernate;
using NHibernate.Metadata;
using NUnit.Framework;
using WebBase.Data.Mapping;
using Shaml.Data.NHibernate;
using Shaml.Testing.NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System.IO;

namespace Tests.WebBase.Data.NHibernateMaps
{
    /// <summary>
    /// Provides a means to verify that the target database is in compliance with all mappings.
    /// Taken from http://ayende.com/Blog/archive/2006/08/09/NHibernateMappingCreatingSanityChecks.aspx.
    /// 
    /// If this is failing, the error will likely inform you that there is a missing table or column
    /// which needs to be added to your database.
    /// </summary>
    [TestFixture]
    [Category("DB Tests")]
    public class MappingIntegrationTests 
    {
        [SetUp]
        public virtual void SetUp() {
            string[] mappingAssemblies = RepositoryTestsHelper.GetMappingAssemblies();
            configuration = NHibernateSession.Init(new SimpleSessionStorage(), mappingAssemblies,
                                   new AutoPersistenceModelGenerator().Generate(),
                                   "../Config/NHibernate.config");
        }

        [TearDown]
        public virtual void TearDown() {
            NHibernateSession.CloseAllSessions();
            NHibernateSession.Reset();
        }

        [Test]
        public void CanConfirmDatabaseMatchesMappings() {
            var allClassMetadata = NHibernateSession.GetDefaultSessionFactory().GetAllClassMetadata();

            foreach (var entry in allClassMetadata) {
                NHibernateSession.Current.CreateCriteria(entry.Value.GetMappedClass(EntityMode.Poco))
                     .SetMaxResults(0).List();
            }
        }

        /// <summary>
        /// Generates and outputs the database schema SQL to the console
        /// </summary>
        [Test]
        public void CanGenerateDatabaseSchema() {
            using (TextWriter stringWriter = new StreamWriter("../DB/Create_Schema.sql")) {
                new SchemaExport(configuration).Execute(x => stringWriter.WriteLine(x+";"), false, false);
            }
        }

        /// <summary>
        /// Generates and outputs the database update SQL to the console
        /// </summary>
        [Test]
        public void CanGenerateDatabaseUpdateSchema()
        {
          using (TextWriter stringWriter = new StreamWriter("../DB/Update_Schema.sql"))
          {
            new SchemaUpdate(configuration).Execute(x => stringWriter.WriteLine(x + ";"), false);
          }
        }

        private Configuration configuration;
    }
}
