#!/usr/bin/env ruby
# Creates the files in lib/shaml/templates and builds the gem

require 'rubygems'
require 'fileutils'
require 'zip/zipfilesystem'
require "rexml/document"

IGNORES = [
  /bin$/,
  /obj$/,
  /TestResults$/,
  /TestResult\.xml$/,
  /drops$/,
  /log\.txt$/,
  /\.VisualState\.xml$/,
  /StyleCop.Cache$/,
  /\.LDF$/,
  /~$/,
  /PrecompiledWeb$/,
  /\.suo$/,
  /\.cache$/,
  /\.user$/,
  /\.tmp$/,
  /\.ldf$/,
  /\.mdf$/,
  /\.swx$/,
  /\.dotest$/,
  /\.sass-cache$/,
  /WebSample/
]

zip_file_name = File.join("lib","shaml","templates","shaml_base_template.dat")
FileUtils::rm_rf(zip_file_name)

# Removing WebSample contents from project files
def xmlsimplify(xml)
  doc = REXML::Document.new xml
  doc.elements.each('//*[contains(@Include,"WebSample")]') do |x|
    x.parent.delete(x)
  end
  doc.to_s
end

# recursively add files and directories to zip file
def search(zip,fname)
  ignored = false
  IGNORES.each do |ign|
    if fname =~ ign then
      ignored = true
    end
  end
  unless ignored
    name = fname.sub("webbase/","");
    if File.directory?(fname) then
      zip.dir.mkdir(name);
      Dir.glob(File.join(fname,"*")).each do |d|
        search(zip,d)
      end
    else
      zip.file.open(name,"wb") do |f|
        File.open(fname,"rb") do |i|        
          output = i.read
          # remove WebSample elements from project files
          if fname =~ /\.csproj$/ then
            output = xmlsimplify(output)
          end
          f.write output
        end
      end
    end
  end  
end

puts "Creating shaml_base_template.dat"

Zip::ZipFile.open(zip_file_name, Zip::ZipFile::CREATE) do |zip|
  Dir.glob("webbase/*").each do |fname|
    search(zip,fname)
  end
end

puts "Creating WebSample template files"
FileUtils::cp(File.join("webbase","App","Controllers","WebSamplesController.cs"),File.join("lib","shaml","templates"))
File.open(File.join("webbase","App","Core","WebSample.cs"),"rb") do |inp|
  File.open(File.join("lib","shaml","templates","WebSample.cs"),"wb+") do |outp|
    outp.write inp.read.gsub("public class PropertyType { }","")
  end
end
FileUtils::cp(File.join("webbase","App","Data","WebSampleMap.cs"),File.join("lib","shaml","templates"))
FileUtils::cp(File.join("webbase","Tests","Controllers","WebSamplesControllerTests.cs"),File.join("lib","shaml","templates"))
FileUtils::cp(File.join("webbase","Tests","Core","WebSampleTests.cs"),File.join("lib","shaml","templates"))
FileUtils::cp(File.join("webbase","App","Views","WebSamples","_WebSampleForm.haml"),File.join("lib","shaml","templates"))
FileUtils::cp(File.join("webbase","App","Views","WebSamples","Create.haml"),File.join("lib","shaml","templates"))
FileUtils::cp(File.join("webbase","App","Views","WebSamples","Edit.haml"),File.join("lib","shaml","templates"))
FileUtils::cp(File.join("webbase","App","Views","WebSamples","Delete.haml"),File.join("lib","shaml","templates"))
FileUtils::cp(File.join("webbase","App","Views","WebSamples","Index.haml"),File.join("lib","shaml","templates"))
FileUtils::cp(File.join("webbase","App","Views","WebSamples","Show.haml"),File.join("lib","shaml","templates"))

puts "Building Gem"
system("gem build shaml.gemspec")
