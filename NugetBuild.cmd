@ECHO off
Setlocal EnableDelayedExpansion

SET /p apiKey=Enter API Key: %=%
.nuget\nuget setApiKey !apiKey!

SET /p deployMonJobs=Would you like to deploy the Core Project? (y/n) %=%
IF (!deployMonJobs!) EQU (y) (
	.nuget\nuget pack MonJobs\MonJobs.csproj -IncludeReferencedProjects -ExcludeEmptyDirectories -Build -Symbols -Properties Configuration=Release
	@ECHO Finished Building: MonJobs

	SET /p monJobsVersion=Enter MonJobs Package Version? %=%
	@ECHO Publishing MonJobs.!monJobsVersion!.nupkg
	.nuget\nuget push MonJobs.!monJobsVersion!.nupkg
)

SET /p deploySubscriptions=Would you like to deploy the Subscriptions Project? (y/n) %=%
IF (!deploySubscriptions!) EQU (y) (
	.nuget\nuget pack MonJobs.Subscriptions\MonJobs.Subscriptions.csproj -IncludeReferencedProjects -ExcludeEmptyDirectories -Build -Symbols -Properties Configuration=Release
	@ECHO Finished Building: MonJobs.Subscriptions

	SET /p subscriptionsVersion=Enter MonJobs Subscriptions Package Version? %=%
	@ECHO Publishing MonJobs.Subscriptions.!subscriptionsVersion!.nupkg
	.nuget\nuget push MonJobs.Subscriptions.!subscriptionsVersion!.nupkg
)

@ECHO on