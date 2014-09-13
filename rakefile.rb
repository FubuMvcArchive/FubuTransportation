require 'fuburake'

solution = FubuRake::Solution.new do |sln|	 
	sln.assembly_info = {
		:product_name => "FubuTransportation",
		:copyright => 'Copyright 2013. All rights reserved.'
	}
	
	#sln.ci_steps = ["st:run"]
	
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

FubuRake::Storyteller.new({
  :path => 'src/FubuTransportation.Serenity.Samples',
  :compilemode => solution.compilemode,
  :prefix => "samples:st"
})