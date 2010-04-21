/**
@mainpage Shaml Architecture
@authors Zsolt Sz Sztupák
@section intro Introduction

The purpose of Shaml Architecture is to allow creating ASP.NET MVC applications under
mono using tools that are available on both Windows and Linux machines. The framework
is based around a ruby command, called shaml, that allows you to easlity execute commands that
you need to build ASP.NET MVC applications, like creating models, controllers, resources, compiling
the application or running the tests.

@section cr shaml command reference
@verbatim
Usage:
  shaml command [parameters]

Where command might be:
 generate
  app AppName             : Create new shaml application
  resource ResName
        {haml|asp} [desc] : Create new CRUD resource with
                            a model, a view and a controller
  controller Controller
        {haml|asp} [desc] : Create a standalone controller
  model Model [desc]      : Create a standalone model

 compile                  : Compiles the solution using {ms|x}build
 server                   : Runs xsp2

 gconsole                 : Starts a gsharp console
 console                  : Starts a csharp console
 runner script_name.cs    : Runs the script
 test                     : Runs the tests
@endverbatim

@subsection app
@verbatim
shaml generate app AppName
@endverbatim
The app command is used to generate a new application from the base template. The new application generated doesn't
contain any models, which is needed to compile the project. Therefore after creating a new app you should also add a new model
or a new resource to the application. The command has one parameter: AppName, which will be the name of the application. To
conform to C# standards you should use CamelCase to name your application.

@subsection resource
@verbatim
shaml generate resource ResourceName [haml|asp] [resource description]
@endverbatim
The resource command will generate a model and a controller in one step with the parameters specified.
Check those commands for more information.

@subsection controller
@verbatim
shaml generate resource ControllerName [haml|asp] [accompaning model description]
@endverbatim
The controller command creates a new CRUD based controller and accompaning views. By default it creates views based on
the NHaml template engine, but if you specify "asp" as the engine in the creation script you'll get standard aspx and aspc
files. The name of the controller should be named using CamelCase. The specification for the the module description is as follows:

@verbatim
   description ::= `"´ desclist `"´
      desclist ::= propertydesc {`;´ desclist }
  propertydesc ::= property_name `:´ property_type
@endverbatim

where property_name and property_type are valid C# variable names, and types. For example:

@verbatim
shaml generate controller User "name:string;email:string;birthdate:DateTime"
@endverbatim

@subsection model
@verbatim
shaml generate model ModelName [model description]
@endverbatim
The model command creates a standalone model with the module description provided. For the specification of the
description check the controller subsecrion

@subsection compile
@verbatim
shaml compile
@endverbatim

Compiles the application's solution using xbuild, if shaml could find the mono executables,
or msbuild, if the mono executables are not found. After C# compilation it runs compass for CSS stylesheet
compilation too.

@subsection server
@verbatim
shaml server [xsp parameters]
@endverbatim
Runs xsp2 with the parameters you specify, like --port

@subsection gconsole
@verbatim
shaml gconsole
@endverbatim

Starts a 'gsharp' session with the application's assemblies preloaded. All dinamically loaded modules should reside
in the gsharp applications directory in order this command to work. (Usually the NHibernate database engine providers
need to be put there like Npgsql.dll or System.Data.SQLite.dll

@subsection console
@verbatim
shaml console
@endverbatim

Starts a 'csharp' session with the application's assemblies preloaded. All dinamically loaded modules should reside
in the 'csharp' applications directory (usually /usr/lib/mono/2.0) in order this command to work. (Usually the NHibernate database engine providers
need to be put there like Npgsql.dll or System.Data.SQLite.dll.

@subsection runner
@verbatim
shaml runner [script name]
@endverbatim

Starts a 'csharp' session and runs the script that is specified. There are a few pre-defined scripts to run:

@verbatim
Scripts/run_create_schema.cs  : (re)creates the database schema from scratch
Scripts/run_update_schema.cs  : updates the schema using alter table (doesn't work with PostgreSQL)
Scripts/dump_create_schema.cs : dumps the creation SQL scripts to the DB directory
Scripts/dump_update_schema.cs : dumps the update SQL scripts to the DB directory
@endverbatim

@subsection tests
@verbatim
shaml test
@endverbatim

Runs the tests using NUnit.

@section base Base template specification

@subsection dir Directory structure of the base template

@verbatim
├───App
│   ├───ApplicationServices : Application Services
│   ├───Controllers : Controllers
│   │   └───Support : Support Controllers, like MembershipAdministration
│   ├───Core : Models (Domain)
│   ├───Data : Fluent NHibernate AutoMapping overrides
│   │   ├───Mapping
│   │   │   └───Conventions : Default conventions for AutoMapping
│   ├───Stylesheets : Compass sass stylesheet sources
│   └───Views : Views
│       ├───Account
│       ├───Home
│       ├───MembershipAdministration
│       ├───OpenId
│       ├───Shared
│       └───WebSamples
├───Config : Configuration files like routing configuration or database configuration
├───DB : Database and schema creation/update scripts
├───libraries : Referenced Assemblies
├───logs : Log4net logs
├───Public : Static contents, like scripts, images and generated CSS files
├───Scripts : CSharp scripts for the runner command
│   └───setup : Setup files for the console/gconsole/runner commands
└───Tests : NUnit Tests
@endverbatim

@subsection Projects

@subsubsection ProjectName ProjectName

Contains the views and the setup commands needed for ASP.NET MVC to start

@subsubsection ApplicationServices ProjectName.ApplicationServices

Contains application services

@subsubsection Core ProjectName.Core

Contains the models

@subsubsection Data ProjectName.Data

Contains the auto mapping conventions, and auto mapping overrides for the ORM

@subsubsection Config ProjectName.Config

Contains the configuration files

@subsubsection Tests ProjectName.Tests

Contains the tests
*/

