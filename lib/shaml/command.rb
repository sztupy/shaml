require 'zip/zip'
require 'rexml/document'

Script = <<"SCRIPT"
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
SCRIPT

module Shaml
class CommandLoader
  def initialize
  end

  def getappname
    appname = Dir.glob("*.sln").first
    if appname.nil? 
      puts 'S#aml ERROR: solution file not found. Change directory to a s#aml-architecture project'
      exit
    end
    appname.gsub(".sln","")
  end

  def camelcase(phrase)
    phrase.gsub(/^[a-z]|[ _]+[a-z]/) { |a| a.upcase }.gsub(/[ _]/, '')
  end

  def unzip_file(file, destination)
    Zip::ZipFile.open(file) { |zip_file|
     zip_file.each { |f|
       f_path=File.join(destination, f.name)
       FileUtils.mkdir_p(File.dirname(f_path))
       zip_file.extract(f, f_path) unless File.exist?(f_path)
     }
    }
  end

  def convert_file(file, appname, modelname, propertydescriptor = nil)
    out = ""
    pstring = ""
    propertydescriptor = "" if propertydescriptor.nil?
    insideprop = false
    file.each_line do |line|
      if insideprop then
        if line.strip =~ /__END__PROPERTY__/ then
          propertydescriptor.split(";").each do |property|
            p = property.split(":")
            out << pstring.gsub("PropertyType",p[1]).gsub("Property",p[0])
          end
          insideprop = false
        else
          pstring << line.gsub("WebBase",appname).gsub("WebSample", camelcase(modelname)).gsub("websample", modelname);
        end
      else
        if line.strip =~ /__BEGIN__PROPERTY__/ then
          pstring = ""
          insideprop = true
        else
          out << line.gsub("WebBase",appname).gsub("WebSample", camelcase(modelname)).gsub("websample", modelname);
        end
      end
    end
    out
  end


  def copy_file(from,to,appname,modelname,propertydescriptor,xmltochange,xmltype)
    outfname = to.gsub("WebSample",camelcase(modelname))
    outfname_w = outfname.gsub("/","\\")
    addxml(xmltochange,outfname_w,xmltype)
    FileUtils.mkdir_p(File.dirname(outfname))
    File.open(from,"rb") do |infile|
      File.open(outfname,"wb+") do |outfile|
        puts "Writing #{outfname}"        
        outfile.write convert_file(infile.read,appname,modelname,propertydescriptor)
      end
    end
  end

  # Fix files that depends on the existence of at least one model
  def fix_with_model(modelname)
    t = nil
    appname = getappname
    name = File.join("App","Data","Mapping","AutoPersistenceModelGenerator.cs")
    File.open(name,"rb") { |f| t = f.read }; File.open(name,"wb+") { |f| f.write convert_file(t,appname,modelname) }
    name = File.join("Config","ComponentRegistrar.cs")
    File.open(name,"rb") { |f| t = f.read }; File.open(name,"wb+") { |f| f.write convert_file(t,appname,modelname) }
  end

  def addxml(file,content,type)
    doc = nil
    dir = File.split(file)[0].gsub("/","\\")
    c = content.gsub(dir+"\\","")
    File.open(file,"r") do |f|
      doc = REXML::Document.new f.read
      case type
      when :content
        el = doc.elements["//ItemGroup[Content]"]
        cont = el.add_element("Content",{"Include"=>c})
        stype = cont.add_element("SubType")
        stype.text = "ASPXCodeBehind"
      when :compile
        el = doc.elements["//ItemGroup[Compile]"]
        cont = el.add_element("Compile",{"Include"=>c})
      end
    end
    File.open(file,"w+") do |f|
      f.write doc.to_s
    end
  end

  def run
    if ARGV.length == 0 then
      puts "S#aml architecture: ASP.NET MVC 2 and NHibernate 2.1 on mono 2.4.4+"
      puts " version: #{SHAML_VERSION}"
      puts
      puts "Usage:"
      puts "  shaml command [parameters]"
      puts 
      puts "Where command might be:"
      puts " generate"
      puts "  app AppName             : Create new shaml application"
      puts "  resource ResName [desc] : Create new CRUD resource with"
      puts "                            a model, a view and a controller"
      puts "  controller Controller   : Create a standalone controller"
      puts "  model Model [desc]      : Create a standalone model"
      puts
      puts " compile                  : Compiles the solution using xbuild"
      puts " server                   : Runs xsp2"
      puts
      puts " console                  : Starts a csharp console"
      puts " runner script_name.cs    : Runs the script"
      puts
      puts "Examples: "
      puts "  shaml generate app Blog"
      puts "  shaml generate resource Post"
      puts "  shaml compile"
      puts
      puts "The console and runner parameters will preload the solutions"
      puts "assemblies and configuration files, and loads everything you need to get"
      puts "working with the domain objects"
      puts
      puts "The optional [desc] parameter describes the base schema of the model to"
      puts "create the scaffold. Here is an example how it looks like:"
      puts "  name:string;email:string;birthdate:DateTime" 
    else
      command = ARGV.shift
      case command
      when "generate" then
        type = ARGV.shift
        name = ARGV.shift
        if name then
          case type
          when "app" then
            unzip_file(File.join(TEMPLATEDIR,"shaml_base_template.dat"), ".shaml_extract_temp")
            Dir.glob(".shaml_extract_temp/**/*").each do |filename|
              infname = filename
              outfname = filename.gsub("WebBase",name).gsub(".shaml_extract_temp",name);
              FileUtils.mkdir_p(File.dirname(outfname))
              unless File.directory?(infname)
                File.open(infname,"rb") do |infile|         
                  File.open(outfname,"wb+") do |outfile|
                    puts "Writing #{outfname}"
                    if infname=~/\.dll/ then
                      outfile.write infile.read                 
                    else
                      outfile.write infile.read.gsub("WebBase",name);
                    end
                  end
                end
              end
            end
            FileUtils.rm_rf ".shaml_extract_temp"
          when "resource"
            desc = ARGV.shift || nil
            appname = getappname
            xmlname = File.join("App","Core","#{appname}.Core.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSample.cs"),File.join("App","Core","WebSample.cs"),appname,name,desc,xmlname,:compile)
            xmlname = File.join("App","Controllers","#{appname}.Controllers.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSamplesController.cs"),File.join("App","Controllers","WebSamplesController.cs"),appname,name,desc,xmlname,:compile)
            xmlname = "#{appname}.csproj"            
            copy_file(File.join(TEMPLATEDIR,"_WebSampleForm.haml"),File.join("App","Views","WebSamples","_WebSampleForm.haml"),appname,name,desc,xmlname,:content)
            copy_file(File.join(TEMPLATEDIR,"Create.haml"),File.join("App","Views","WebSamples","Create.haml"),appname,name,desc,xmlname,:content)
            copy_file(File.join(TEMPLATEDIR,"Delete.haml"),File.join("App","Views","WebSamples","Delete.haml"),appname,name,desc,xmlname,:content)
            copy_file(File.join(TEMPLATEDIR,"Edit.haml"),File.join("App","Views","WebSamples","Edit.haml"),appname,name,desc,xmlname,:content)
            copy_file(File.join(TEMPLATEDIR,"Index.haml"),File.join("App","Views","WebSamples","Index.haml"),appname,name,desc,xmlname,:content)
            copy_file(File.join(TEMPLATEDIR,"Show.haml"),File.join("App","Views","WebSamples","Show.haml"),appname,name,desc,xmlname,:content)
            xmlname = File.join("Tests","#{appname}.Tests.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSampleTests.cs"),File.join("Tests","Core","WebSampleTests.cs"),appname,name,desc,xmlname,:compile)        
            copy_file(File.join(TEMPLATEDIR,"WebSamplesControllerTests.cs"),File.join("Tests","Controllers","WebSamplesControllerTests.cs"),appname,name,desc,xmlname,:compile)
            fix_with_model(name)
          when "model"
            desc = ARGV.shift || nil
            appname = getappname
            xmlname = File.join("App","Core","#{appname}.Core.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSample.cs"),File.join("App","Core","WebSample.cs"),appname,name,desc,xmlname,:compile)
            xmlname = File.join("Tests","#{appname}.Tests.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSampleTests.cs"),File.join("Tests","Core","WebSampleTests.cs"),appname,name,desc,xmlname,:compile)
            fix_with_model(name)            
          when "controller"
            desc = ARGV.shift || nil
            appname = getappname   
            xmlname = File.join("App","Controllers","#{appname}.Controllers.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSamplesController.cs"),File.join("App","Controllers","WebSamplesController.cs"),appname,name,desc,xmlname,:compile)
            xmlname = "#{appname}.csproj"            
            copy_file(File.join(TEMPLATEDIR,"_WebSampleForm.haml"),File.join("App","Views","WebSamples","_WebSampleForm.haml"),appname,name,desc,xmlname,:content)
            copy_file(File.join(TEMPLATEDIR,"Create.haml"),File.join("App","Views","WebSamples","Create.haml"),appname,name,desc,xmlname,:content)
            copy_file(File.join(TEMPLATEDIR,"Delete.haml"),File.join("App","Views","WebSamples","Delete.haml"),appname,name,desc,xmlname,:content)
            copy_file(File.join(TEMPLATEDIR,"Edit.haml"),File.join("App","Views","WebSamples","Edit.haml"),appname,name,desc,xmlname,:content)
            copy_file(File.join(TEMPLATEDIR,"Index.haml"),File.join("App","Views","WebSamples","Index.haml"),appname,name,desc,xmlname,:content)
            copy_file(File.join(TEMPLATEDIR,"Show.haml"),File.join("App","Views","WebSamples","Show.haml"),appname,name,desc,xmlname,:content)
            xmlname = File.join("Tests","#{appname}.Tests.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSamplesControllerTests.cs"),File.join("Tests","Controllers","WebSamplesControllerTests.cs"),appname,name,desc,xmlname,:compile)
          else
            puts 'S#aml ERROR: unknown generate argument'
          end        
        else
          puts 'S#aml ERROR: no name specified'
        end
      when "compile"
        appname = getappname
        puts "Copying libraries"
        begin
          FileUtils.rm_r(File.join(appname,"bin"))
        rescue Exception => e
        end
        FileUtils.cp_r("libraries",File.join(appname,"bin"))
        puts "Compiling using xbuild"
        system("xbuild #{appname}.sln")
        puts "Compiling stylesheets"        
        Dir.chdir(appname) do    
          system("compass --update -c Config/compass_config.rb")
        end
      when "server"
        puts "Starting xsp2"    
        appname = getappname
        Dir.chdir(appname) do
          puts "Changed directory to #{Dir.pwd}"
          puts "Starting xsp2 #{ARGV.join(" ")}"
          system("xsp2 #{ARGV.join(" ")}")
          puts "Done..."      
        end
      when "console"
        appname = getappname
        script = Script + <<SCRIPT
System.Console.WriteLine();
help;
System.Console.WriteLine();
System.Console.WriteLine("Welcome to the S#aml interactive C# console!");
System.Console.WriteLine();
System.Console.WriteLine("An NHibernate ISession is already opened for you");
System.Console.WriteLine("To start a query type:");
System.Console.WriteLine("  var c = s.CreateQuery(\\"from WebSample\\");");
System.Console.WriteLine("  c.List<WebSample>();\");
System.Console.WriteLine();
SCRIPT
        script = script.gsub("WebBase",appname);
        Mono.load_csharp(script,"-lib:bin -r:FluentNHibernate.dll -r:Iesi.Collections.dll -r:LinFu.Core.dll -r:LinFu.DynamicProxy.dll -r:log4net.dll -r:Microsoft.Practices.ServiceLocation.dll -r:Mono.Security.dll -r:Newtonsoft.Json.dll -r:NHaml.dll -r:NHaml.Web.Mvc.dll -r:NHibernate.dll -r:NHibernate.ByteCode.LinFu.dll -r:NHibernate.Validator.dll -r:Npgsql.dll -r:Shaml.Core.dll -r:Shaml.Core.Validator.dll -r:Shaml.Data.dll -r:Shaml.Membership.dll -r:Shaml.Testing.dll -r:Shaml.Tests.dll -r:Shaml.Web.dll -r:WebBase.dll -r:WebBase.ApplicationServices.dll -r:WebBase.Config.dll -r:WebBase.Controllers.dll -r:WebBase.Core.dll -r:WebBase.Data.dll -r:WebBase.Tests.dll");
      when "runner"
        appname = getappname
        command = ARGV.shift
        script = Script.gsub("WebBase",appname);
        Mono.load_csharp(script,"-lib:bin -r:FluentNHibernate.dll -r:Iesi.Collections.dll -r:LinFu.Core.dll -r:LinFu.DynamicProxy.dll -r:log4net.dll -r:Microsoft.Practices.ServiceLocation.dll -r:Mono.Security.dll -r:Newtonsoft.Json.dll -r:NHaml.dll -r:NHaml.Web.Mvc.dll -r:NHibernate.dll -r:NHibernate.ByteCode.LinFu.dll -r:NHibernate.Validator.dll -r:Npgsql.dll -r:Shaml.Core.dll -r:Shaml.Core.Validator.dll -r:Shaml.Data.dll -r:Shaml.Membership.dll -r:Shaml.Testing.dll -r:Shaml.Tests.dll -r:Shaml.Web.dll -r:WebBase.dll -r:WebBase.ApplicationServices.dll -r:WebBase.Config.dll -r:WebBase.Controllers.dll -r:WebBase.Core.dll -r:WebBase.Data.dll -r:WebBase.Tests.dll #{command}");
      else
        puts 'S#aml ERROR: unknown command'
      end
    end
  end
end
end
