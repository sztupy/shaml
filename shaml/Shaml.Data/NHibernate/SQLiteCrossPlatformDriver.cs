using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Dialect;
using FluentNHibernate.Cfg.Db;

namespace Shaml.Data.NHibernate
{
  [CLSCompliant(false)]
  public class SQLiteCrossPlatformConfiguration : PersistenceConfiguration<SQLiteCrossPlatformConfiguration>
  {
    public static SQLiteCrossPlatformConfiguration Standard
    {
      get { return new SQLiteCrossPlatformConfiguration(); }
    }

    public SQLiteCrossPlatformConfiguration()
    {
      Driver<SQLiteCrossPlatformDriver>();
      Dialect<SQLiteDialect>();
      Raw("query.substitutions", "true=1;false=0");
    }

    public SQLiteCrossPlatformConfiguration InMemory()
    {
      Raw("connection.release_mode", "on_close");
      return ConnectionString(c => c
          .Is("Data Source=:memory:;Version=3;New=True;"));

    }

    public SQLiteCrossPlatformConfiguration UsingFile(string fileName)
    {
      return ConnectionString(c => c
          .Is(string.Format("Data Source={0};Version=3;New=True;", fileName)));
    }

    public SQLiteCrossPlatformConfiguration UsingFileWithPassword(string fileName, string password)
    {
      return ConnectionString(c => c
          .Is(string.Format("Data Source={0};Version=3;New=True;Password={1};", fileName, password)));
    }
  }

  class SQLiteCrossPlatformDriver : global::NHibernate.Driver.ReflectionBasedDriver
  {
		/// <summary>
    /// Initializes a new instance of <see cref="SQLiteCrossPlatformDriver"/> using either Mono.Data.SQLite or System.Data.SQLite.
		/// </summary>
		/// <exception cref="HibernateException">
		/// Thrown when the assemblies can not be loaded.
		/// </exception>
    public SQLiteCrossPlatformDriver() : base(
      Type.GetType("Mono.Runtime") != null ? "Mono.Data.Sqlite" : "System.Data.SQLite",
      Type.GetType("Mono.Runtime") != null ? "Mono.Data.Sqlite.SqliteConnection" : "System.Data.SQLite.SQLiteConnection",
      Type.GetType("Mono.Runtime") != null ? "Mono.Data.Sqlite.SqliteCommand" : "System.Data.SQLite.SQLiteCommand")
		{
		}

		public override bool UseNamedPrefixInSql
		{
			get { return true; }
		}

		public override bool UseNamedPrefixInParameter
		{
			get { return true; }
		}

		public override string NamedPrefix
		{
			get { return "@"; }
		}

		public override bool SupportsMultipleOpenReaders
		{
			get { return false; }
		}
		
		public override bool SupportsMultipleQueries
		{
			get { return true; }
		}
  }
}
