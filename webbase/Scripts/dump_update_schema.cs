using (TextWriter stringWriter = new StreamWriter("DB/Update_Schema.sql")) {
  new SchemaUpdate(configuration).Execute(x => stringWriter.WriteLine(x+";"), false);
}
