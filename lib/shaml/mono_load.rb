require 'rbconfig'
require 'fileutils'

module Shaml
class MonoLoader
  attr_reader :mono_found,:is_unix

  def initialize
    @mono_command = '/bin/mono'
    @mono_directory = '/usr'
    @mono_lib_directory = '/usr/lib'
    @csharp_command = '/mono/2.0/csharp.exe'
    @gsharp_command = '/gsharp/gsharp.exe'
    @config_dir = "#{ENV['HOME']}/.config"
    @init_script_name = 'shaml.cs'
    @mono_found = true
    @is_unix = true

    # get mono path for Windows
    if RbConfig::CONFIG['host_os'] =~ /mswin|windows|cygwin|mingw/i
      begin
        @is_unix = false
        require 'win32/registry'
        # Check in default location
        base = "Software\\Novell\\Mono\\"
        begin
          Win32::Registry::HKEY_LOCAL_MACHINE.open(base) do |reg|
          end
        # if running ruby 64-bit and mono 32-bit, change the registry search location
        rescue Win32::Registry::Error
          base = "Software\\Wow6432Node\\Novell\\Mono\\"
        end
        Win32::Registry::HKEY_LOCAL_MACHINE.open(base) do |reg|
          monoversion = reg.read_s("DefaultCLR")
          reg.open(monoversion) do |r|
            @mono_directory = r.read_s("SdkInstallRoot")
            @mono_lib_directory = r.read_s("FrameworkAssemblyDirectory")
            @mono_command = "\\bin\\mono"
          end
        end
        Win32::Registry::HKEY_CURRENT_USER.open("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\User Shell Folders") do |reg|
          @config_dir = reg.read_s_expand("AppData")
        end
      rescue Exception => e
        STDERR.puts "Couldn't determine mono location under Windows!"
        STDERR.puts e.inspect
        @mono_found = false
      end
    else
      if !ENV['MONO_PREFIX'].nil? and !ENV['MONO_PREFIX'].empty? then
        @mono_directory = ENV['MONO_PREFIX']
        @mono_lib_directory = File.join(@mono_directory,"lib")
      end
    end
  end

  # create a file that will be executed before the interpreter starts
  def create_init_script(init_script, name)
    if init_script then
      csconfig = File.join(@config_dir,name)
      FileUtils::mkdir_p(csconfig)
      csfile = File.join(csconfig,@init_script_name);
      File.open(csfile,"w+") do |f|
        f.write init_script
      end
    end    
  end

  # remove the file
  def delete_init_script(init_script, name)
    if init_script then
      csconfig = File.join(@config_dir,name)
      csfile = File.join(csconfig,@init_script_name);      
      FileUtils::rm_rf(csfile)
    end    
  end

  # run csharp with an optional initialization script
  def load_csharp(init_script = nil, commands = "")
    create_init_script(init_script,"csharp")
    puts "Mono executable: \"#{File.join(@mono_directory,@mono_command)}\""
    cs = File.join(@mono_lib_directory,@csharp_command)
    puts "CSharp executable: \"#{cs}\""
    system("\"#{File.join(@mono_directory,@mono_command)}\" \"#{cs}\" #{commands}")
    delete_init_script(init_script, "csharp")
  end

  # run gsharp with an optional initialization script
  def load_gsharp(init_script = nil, commands = "")
    create_init_script(init_script,"gsharp")
    puts "Mono executable: \"#{File.join(@mono_directory,@mono_command)}\""
    gs = File.join(@mono_lib_directory,@gsharp_command)
    puts "GSharp executable: \"#{gs}\""
    system("\"#{File.join(@mono_directory,@mono_command)}\" \"#{gs}\" #{commands}")
    delete_init_script(init_script, "gsharp") 
  end

  def load_mono_app(command,parameters = "")
    cm = File.join(@mono_lib_directory,command)
    system("\"#{File.join(@mono_directory,@mono_command)}\" \"#{cm}\" #{parameters}")
  end
  def load_app(command,parameters = "")
    cm = command
    system("\"#{File.join(@mono_directory,@mono_command)}\" \"#{cm}\" #{parameters}")
  end  
end
end
