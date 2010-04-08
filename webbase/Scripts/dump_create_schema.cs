using (TextWriter stringWriter = new StreamWriter("DB/Create_Schema.sql")) {
  new SchemaExport(configuration).Execute(x => stringWriter.WriteLine(x+";"), false, false);
}
