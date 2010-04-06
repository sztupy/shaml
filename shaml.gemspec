Gem::Specification.new do |s|
  s.name = %q{shaml}
  s.version = "0.5.0.alpha"

  s.required_rubygems_version = Gem::Requirement.new(">= 0") if s.respond_to? :required_rubygems_version=
  s.authors = ["Zsolt Sz. Sztupak"]
  s.date = %q{2010-04-01}
  s.default_executable = %q{shaml}
  s.description = %q{Shaml is an ASP.NET MVC 2 framework with NHibernate 2.1 for mono 2.4.4+}
  s.email = %q{mail@sztupy.hu}
  s.executables = ["shaml"]
  s.files = ["bin/shaml","lib/shaml/templates/shaml_base_template.dat"] + 
            Dir.glob("lib/**/*.rb") + 
            Dir.glob("lib/**/*.haml") + 
            Dir.glob("lib/**/*.cs")
  s.homepage = %q{http://code.google.com/p/shaml-architecture/}
  s.rdoc_options = ["--charset=UTF-8"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{shaml}
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{ASP.NET MVC on mono}
  s.test_files = [  ]
 
  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 3
 
    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<rubyzip>, [">= 0.9.1"])
      s.add_runtime_dependency(%q<compass>, [">= 0.6.15"])
    else
      s.add_dependency(%q<rubyzip>, [">= 0.9.1"])
      s.add_dependency(%q<compass>, [">= 0.6.15"])
    end
  else
    s.add_dependency(%q<rubyzip>, [">= 0.9.1"])
    s.add_dependency(%q<compass>, [">= 0.6.15"])
  end
end
