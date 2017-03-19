@ECHO off
Setlocal EnableDelayedExpansion

SET /p apiKey=Enter API Key: %=%
.nuget\nuget setApiKey !apiKey! -source https://www.nuget.org

SET /p deployMonJobs=Would you like to deploy the Core MonJobs Project? (y/n) %=%
IF (!deployMonJobs!) EQU (y) (
	.nuget\nuget pack MonJobs\MonJobs.csproj -IncludeReferencedProjects -ExcludeEmptyDirectories -Build -Symbols -Properties Configuration=Release
	@ECHO Finished Building: MonJobs

	SET /p monJobsVersion=Enter MonJobs Package Version? %=%
	@ECHO Publishing MonJobs.!monJobsVersion!.nupkg
	.nuget\nuget push MonJobs.!monJobsVersion!.nupkg -Source https://www.nuget.org/api/v2/package
)

@ECHO on