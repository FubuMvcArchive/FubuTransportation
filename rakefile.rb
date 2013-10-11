begin
  require 'bundler/setup'
  require 'fuburake'
rescue LoadError
  puts 'Bundler and all the gems need to be installed prior to running this rake script. Installing...'
  system("gem install bundler --source http://rubygems.org")
  sh 'bundle install'
  system("bundle exec rake", *ARGV)
  exit 0
end


solution = FubuRake::Solution.new do |sln|
	sln.compile = {
		:solutionfile => 'src/FubuTransportation.sln'
	}
				 
	sln.assembly_info = {
		:product_name => "FubuTransportation",
		:copyright => 'Copyright 2013. All rights reserved.'
	}
	
	sln.ripple_enabled = true
	sln.fubudocs_enabled = true
end

FubuRake::BottleServices.new({
  :dir => "src/DiagnosticsHarness/bin/#{solution.compilemode}", 
  :name => 'ft-harness', 
  :local_service => true,
  :manual => true
})

FubuRake::MvcApp.new({:directory => 'src/DiagnosticsHarness', :name => 'harness'})

# You may have to change the value of :compilemode below to reflect
# your rake file
FubuRake::Storyteller.new({
  :path => 'src/FubuTransportation.Storyteller',
  :compilemode => solution.compilemode
})