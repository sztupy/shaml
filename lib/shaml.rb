require 'rubygems'
require 'zip/zip'

SHAML_VERSION="0.1.6"

TEMPLATEDIR = File.join(File.dirname(__FILE__),"templates")

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

def convert_file(file, appname, modelname, propertydescriptor)
  out = ""
  pstring = ""
  insideprop = false
  file.each_line do |line|
    if insideprop then
      if line.strip=="__END__PROPERTY__" then
        propertydescriptor.split(";").each do |property|
          p = property.split(":")
          out << pstring.gsub("PropertyType",p[1]).gsub("Property",p[0])
        end
        insideprop = false
      else
        pstring << line.gsub("WebBase",appname).gsub("WebSample", camelcase(modelname)).gsub("websample", modelname);
      end
    else
      if line.strip=="__BEGIN__PROPERTY__" then
        pstring = ""
        insideprop = true
      else
        out << line.gsub("WebBase",appname).gsub("WebSample", camelcase(modelname)).gsub("websample", modelname);
      end
    end
  end
  out
end

def copy_file(from,to,appname,modelname,propertydescriptor)
  
  outfname = to.gsub("WebSample",camelcase(modelname))
  FileUtils.mkdir_p(File.dirname(outfname))
  File.open(from,"rb") do |infile|
    File.open(outfname,"wb+") do |outfile|
      puts "Writing #{outfname}"        
      outfile.write convert_file(infile.read,appname,modelname,propertydescriptor)
    end
  end
end

if ARGV.count == 0 then
  puts "S#aml architecture: ASP.NET MVC and NHibernate on mono 2.4+"
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
  puts " compile                  : Compiles the solution under mono"
  puts
  puts " server                   : Runs xsp2"
  puts
  puts "Examples: "
  puts "  shaml generate app Blog"
  puts "  shaml generate resource Post"
  puts "  shaml compile"
  puts
  puts "The optional [desc] parameter describes the base schema"
  puts "of the model. Here is an example how it looks like:"
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
        unzip_file(File.join(TEMPLATEDIR,"shaml_base_template.zip"), ".shaml_extract_temp")
        Dir.glob(".shaml_extract_temp/**/*").each do |filename|
          infname = filename
          outfname = filename.gsub("WebBase",name).gsub(".shaml_extract_temp",name);
          FileUtils.mkdir_p(File.dirname(outfname))
          unless File.directory?(infname)
            File.open(infname,"rb") do |infile|         
              File.open(outfname,"wb+") do |outfile|
                puts "Writing #{outfname}"
                outfile.write infile.read.gsub("WebBase",name);
              end
            end
          end
        end
        FileUtils.rm_rf ".shaml_extract_temp"
      when "resource"
        desc = ARGV.shift || nil
        appname = getappname
        copy_file(File.join(TEMPLATEDIR,"WebSample.cs"),File.join(appname,"App","Models","WebSample.cs"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"WebSamplesController.cs"),File.join(appname,"App","Controllers","WebSamplesController.cs"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"_WebSampleForm.haml"),File.join(appname,"App","Views","WebSample","_WebSampleForm.haml"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"Create.haml"),File.join(appname,"App","Views","WebSample","Create.haml"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"Delete.haml"),File.join(appname,"App","Views","WebSample","Delete.haml"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"Edit.haml"),File.join(appname,"App","Views","WebSample","Edit.haml"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"Index.haml"),File.join(appname,"App","Views","WebSample","Index.haml"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"Show.haml"),File.join(appname,"App","Views","WebSample","Show.haml"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"WebSampleTests.cs"),File.join(appname+".Tests","Tests","Core","WebSampleTests.cs"),appname,name,desc)        
        copy_file(File.join(TEMPLATEDIR,"WebSamplesControllerTests.cs"),File.join(appname+".Tests","Tests","Web","Controllers","WebSamplesControllerTests.cs"),appname,name,desc)        
      when "model"
        desc = ARGV.shift || nil
        appname = getappname      
        copy_file(File.join(TEMPLATEDIR,"WebSample.cs"),File.join(appname,"App","Models","WebSample.cs"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"WebSampleTests.cs"),File.join(appname+".Tests","Tests","Core","WebSampleTests.cs"),appname,name,desc)        
      when "controller"
        desc = ARGV.shift || nil
        appname = getappname   
        copy_file(File.join(TEMPLATEDIR,"WebSamplesController.cs"),File.join(appname,"App","Controllers","WebSamplesController.cs"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"_WebSampleForm.haml"),File.join(appname,"App","Views","WebSample","_WebSampleForm.haml"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"Create.haml"),File.join(appname,"App","Views","WebSample","Create.haml"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"Delete.haml"),File.join(appname,"App","Views","WebSample","Delete.haml"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"Edit.haml"),File.join(appname,"App","Views","WebSample","Edit.haml"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"Index.haml"),File.join(appname,"App","Views","WebSample","Index.haml"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"Show.haml"),File.join(appname,"App","Views","WebSample","Show.haml"),appname,name,desc)
        copy_file(File.join(TEMPLATEDIR,"WebSamplesControllerTests.cs"),File.join(appname+".Tests","Tests","Web","Controllers","WebSamplesControllerTests.cs"),appname,name,desc)                
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
    begin    
      FileUtils.rm_r(File.join(appname+".Tests","bin"))    
    rescue Exception => e
    end    
    FileUtils.cp_r("libraries",File.join(appname,"bin"))
    FileUtils.cp_r("libraries",File.join(appname+".Tests","bin"))    
    puts "Compiling using gmcs"    
    system("gmcs -recurse:#{File.join(appname,"*.cs")} `ls libraries/*.dll | sed \"s/libr/-r:libr/\"` -r:System.Web.Routing -r:System.Web -t:library -out:#{File.join(appname,"bin",appname+".dll")}")
    system("gmcs -recurse:#{File.join(appname+".Tests","*.cs")} `ls libraries/*.dll | sed \"s/libr/-r:libr/\"` -r:System.Web.Routing -r:System.Web -t:library -out:#{File.join(appname+".Tests","bin",appname+".dll")}")
  when "server"
    puts "Starting xsp2"    
    appname = getappname
    Dir.chdir(appname) do
      puts "Changed directory to #{Dir.pwd}"
      ENV["MONO_PATH"] = File.join(Dir.pwd,"bin")
      puts "Set MONO_PATH to #{ENV["MONO_PATH"]}"
      puts "Starting xsp2"
      system("xsp2")
      puts "Done..."      
    end
  else
    puts 'S#aml ERROR: unknown command'
  end
end
