S#aml architecture base project
-------------------------------

The project contains the following directories:
- libraries: The required DLL's
- Shaml.Core: Core extension libraries for ASP.NET MVC
- Shaml.Testing: Testing extension libraries
- WebBase: Sample MVC application scaffold
- WebBase.Test: Sample MVC Test application scaffold

To create the "shaml_base_template.zip" that will be used by shaml, zip WebBase and WebBaste.Test directories,
and add the solution file from WebBase.Generators.

This project is intented for developing the S#aml architecture base files. If you want to create a new project
based on S#aml, then use the "shaml generate app AppName" ruby command to generate an application.
