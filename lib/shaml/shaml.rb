require 'rubygems'
require 'shaml/mono_load'
require 'shaml/command'

SHAML_VERSION="0.5.0"
TEMPLATEDIR = File.join(File.dirname(__FILE__),"templates")
Mono = MonoLoader.new
Command = CommandLoader.new
Command.run
