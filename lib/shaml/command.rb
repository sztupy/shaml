require 'zip/zip'
require 'rexml/document'

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
       if f.directory? then
         FileUtils.mkdir_p(f_path)
       else
         FileUtils.mkdir_p(File.dirname(f_path))
         zip_file.extract(f, f_path) unless File.exist?(f_path)
       end
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
      puts "  resource ResName "
      puts "        {haml|asp} [desc] : Create new CRUD resource with"
      puts "                            a model, a view and a controller"
      puts "  controller Controller "
      puts "        {haml|asp} [desc] : Create a standalone controller"
      puts "  model Model [desc]      : Create a standalone model"
      puts
      puts " compile                  : Compiles the solution using {ms|x}build"
      puts " server                   : Runs xsp2"
      puts
      puts " gconsole                 : Starts a gsharp console"      
      puts " console                  : Starts a csharp console"
      puts " runner script_name.cs    : Runs the script"
      puts " test                     : Runs the tests"
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
              if File.directory?(infname) then
                FileUtils.mkdir_p(outfname)
              else
                FileUtils.mkdir_p(File.dirname(outfname))
              end
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
            puts
            puts "Your S#aml-architecture web application is created"
            puts "Before compilation you need to create a model or resource using"
            puts "   shaml generate resource ResourceName [resourcedescriptors]"
            puts
            puts "After generation fix it's tests and run \"shaml compile\","
            puts "\"shaml runner Scripts/run_create_schema.cs\" and \"shaml server\""
            puts
          when "resource"
            desc = ARGV.shift || nil
            type = "haml"
            if desc=~/^asp([x])?/ then
              type="asp"
              desc = ARGV.shift || nil
            elsif desc=~/^haml/ then
              desc = ARGV.shift || nil
            end
            appname = getappname
            xmlname = File.join("App","Core","#{appname}.Core.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSample.cs"),File.join("App","Core","WebSample.cs"),appname,name,desc,xmlname,:compile)
            xmlname = File.join("App","Data","#{appname}.Data.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSampleMap.cs"),File.join("App","Data","WebSampleMap.cs"),appname,name,desc,xmlname,:compile)            
            xmlname = File.join("App","Controllers","#{appname}.Controllers.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSamplesController.cs"),File.join("App","Controllers","WebSamplesController.cs"),appname,name,desc,xmlname,:compile)
            xmlname = "#{appname}.csproj"
            if type=="haml" then
              copy_file(File.join(TEMPLATEDIR,"_WebSampleForm.haml"),File.join("App","Views","WebSamples","_WebSampleForm.haml"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Create.haml"),File.join("App","Views","WebSamples","Create.haml"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Delete.haml"),File.join("App","Views","WebSamples","Delete.haml"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Edit.haml"),File.join("App","Views","WebSamples","Edit.haml"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Index.haml"),File.join("App","Views","WebSamples","Index.haml"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Show.haml"),File.join("App","Views","WebSamples","Show.haml"),appname,name,desc,xmlname,:content)
            else
              copy_file(File.join(TEMPLATEDIR,"WebSampleForm.ascx"),File.join("App","Views","WebSamples","WebSampleForm.ascx"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Create.aspx"),File.join("App","Views","WebSamples","Create.aspx"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Delete.aspx"),File.join("App","Views","WebSamples","Delete.aspx"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Edit.aspx"),File.join("App","Views","WebSamples","Edit.aspx"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Index.aspx"),File.join("App","Views","WebSamples","Index.aspx"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Show.aspx"),File.join("App","Views","WebSamples","Show.aspx"),appname,name,desc,xmlname,:content)
            end
            xmlname = File.join("Tests","#{appname}.Tests.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSampleTests.cs"),File.join("Tests","Core","WebSampleTests.cs"),appname,name,desc,xmlname,:compile)        
            copy_file(File.join(TEMPLATEDIR,"WebSamplesControllerTests.cs"),File.join("Tests","Controllers","WebSamplesControllerTests.cs"),appname,name,desc,xmlname,:compile)
            fix_with_model(name)
          when "model"
            desc = ARGV.shift || nil
            appname = getappname
            xmlname = File.join("App","Core","#{appname}.Core.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSample.cs"),File.join("App","Core","WebSample.cs"),appname,name,desc,xmlname,:compile)
            xmlname = File.join("App","Data","#{appname}.Data.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSampleMap.cs"),File.join("App","Data","WebSampleMap.cs"),appname,name,desc,xmlname,:compile)                        
            xmlname = File.join("Tests","#{appname}.Tests.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSampleTests.cs"),File.join("Tests","Core","WebSampleTests.cs"),appname,name,desc,xmlname,:compile)
            fix_with_model(name)            
          when "controller"
            desc = ARGV.shift || nil
            type = "haml"
            if desc=~/^asp([x])?/ then
              type="asp"
              desc = ARGV.shift || nil
            elsif desc=~/^haml/ then
              desc = ARGV.shift || nil
            end
            appname = getappname   
            xmlname = File.join("App","Controllers","#{appname}.Controllers.csproj")
            copy_file(File.join(TEMPLATEDIR,"WebSamplesController.cs"),File.join("App","Controllers","WebSamplesController.cs"),appname,name,desc,xmlname,:compile)
            xmlname = "#{appname}.csproj"            
            if type=="haml" then
              copy_file(File.join(TEMPLATEDIR,"_WebSampleForm.haml"),File.join("App","Views","WebSamples","_WebSampleForm.haml"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Create.haml"),File.join("App","Views","WebSamples","Create.haml"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Delete.haml"),File.join("App","Views","WebSamples","Delete.haml"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Edit.haml"),File.join("App","Views","WebSamples","Edit.haml"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Index.haml"),File.join("App","Views","WebSamples","Index.haml"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Show.haml"),File.join("App","Views","WebSamples","Show.haml"),appname,name,desc,xmlname,:content)
            else
              copy_file(File.join(TEMPLATEDIR,"WebSampleForm.ascx"),File.join("App","Views","WebSamples","WebSampleForm.ascx"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Create.aspx"),File.join("App","Views","WebSamples","Create.aspx"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Delete.aspx"),File.join("App","Views","WebSamples","Delete.aspx"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Edit.aspx"),File.join("App","Views","WebSamples","Edit.aspx"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Index.aspx"),File.join("App","Views","WebSamples","Index.aspx"),appname,name,desc,xmlname,:content)
              copy_file(File.join(TEMPLATEDIR,"Show.aspx"),File.join("App","Views","WebSamples","Show.aspx"),appname,name,desc,xmlname,:content)
            end
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
        begin
          FileUtils::rm_rf "bin"
        rescue Exception => e
        end
        if Mono.mono_found then
          puts "Compiling using xbuild"
          Mono.load_mono_app("/mono/2.0/xbuild.exe","#{appname}.sln")
        else
          puts "Mono not found. Compiling using msbuild"
          system("msbuild #{appname}.sln")
        end
        puts "Compiling stylesheets"        
        system("compass -c Config/compass_config.rb")
      when "server"
        appname = getappname
        puts "Starting xsp2 #{ARGV.join(" ")}"
        if Mono.is_unix then
          Mono.load_mono_app("/mono/2.0/xsp2.exe",ARGV.join(" "))
        else
          Mono.load_mono_app("/mono/2.0/WinHack/xsp2.exe",ARGV.join(" "))
        end
        puts "Done..."      
      when "test"
        puts "Running tests"
        appname = getappname
        if Mono.mono_found then
          Mono.load_app(File.join("libraries","nunit-console.exe"),File.join("bin","#{appname}.Tests.dll"))
        else
          system("#{File.join("libraries","nunit-console.exe")} #{File.join("bin","#{appname}.Tests.dll")}")
        end
      when "console"
        appname = getappname
        script = File.read(File.join("Scripts","setup","common.cs"))
        script << File.read(File.join("Scripts","setup","csharp.cs"))
        script = script.gsub("WebBase",appname);
        Mono.load_csharp(script,"");
      when "gconsole"
        appname = getappname
        script = File.read(File.join("Scripts","setup","common.cs"))
        script << File.read(File.join("Scripts","setup","gsharp.cs"))
        script = script.gsub("WebBase",appname);
        Mono.load_gsharp(script,"");
      when "runner"
        appname = getappname
        command = ARGV.shift
        script = File.read(File.join("Scripts","setup","common.cs"))
        script << File.read(File.join("Scripts","setup","runner.cs"))
        script = script.gsub("WebBase",appname);
        Mono.load_csharp(script,"#{command}");
      else
        puts 'S#aml ERROR: unknown command'
      end
    end
  end
end
end
