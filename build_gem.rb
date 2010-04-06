#!/usr/bin/env ruby
# Creates the files in lib/shaml/templates and builds the gem

require 'fileutils'
require 'zip/zipfilesystem'

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
  /\.sass-cache$/
]

zip_file_name = File.join("lib","shaml","templates","shaml_base_template.dat")
FileUtils::rm_rf(zip_file_name)

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
          f.write i.read
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

puts "Building Gem"
system("gem build shaml.gemspec")
