param (
  $output             = $PSScriptRoot,
  $testCsprojPattern  = "*T1.Tests.csproj",
  $configuration      = "Release",
  $frameworkVersion   = "net5.0"
)

$output                = Join-Path $output "\";
$workingRoot           = Join-Path $PSScriptRoot "\";

$sourceDir             = Join-Path $workingRoot "src";
$publishDir            = Join-Path $output "artifacts";
$toolsDir              = Join-Path $workingRoot "tools";
$testPublishDir        = Join-Path $publishDir "t1-publish";
$testResultsDir        = Join-Path $publishDir "t1-results";
$testCoverageDir       = Join-Path $publishDir "t1-coverage";
$coverletExe           = "$toolsDir\coverlet.exe";
$reportGeneratorExe    = "$toolsDir\reportgenerator.exe"


# ==============================================================
# Cleanup
# ==============================================================
#
$cleanupDirectories = @();
$cleanupDirectories += $testPublishDir;
$cleanupDirectories += $testCoverageDir;
$cleanupDirectories += $testResultsDir;
 
foreach ($cleanupDirectory in $cleanupDirectories) {
  if (Test-Path $cleanupDirectory) {
    Remove-item $cleanupDirectory -Recurse -Force | Out-Null;
  }
}

# ==============================================================
# Install tooling
# ==============================================================
#
$installCmd = "";

if(!(Test-Path $coverletExe)){
  Write-Host "Installing: coverlet.console"
  $installCmd = "dotnet tool install coverlet.console --tool-path $toolsDir";
  Invoke-Expression -Command $installCmd -ErrorAction 'Continue';
}

if(!(Test-Path $reportGeneratorExe)){
  Write-Host "Installing: reportgenerator.exe"
  $installCmd = "dotnet tool install dotnet-reportgenerator-globaltool --tool-path `"$toolsDir`"";
  Invoke-Expression -Command $installCmd -ErrorAction 'Continue';
}

# ==============================================================
# Discover and Build test projects
# ==============================================================
#
$testProjects = Get-ChildItem -Path $sourceDir -Include $testCsprojPattern -Recurse -File

if ($testProjects.count -eq 0) {
  throw "No files matched the $testCsprojPattern pattern. The script cannot continue."
}

$buildCmd          = "";
$currentPublishDir = "";

foreach ($testProject in $testProjects) {
  # Build test project
  $dllFileName         = $testProject.BaseName + ".dll";
  $currentPublishDir   = Join-Path $testPublishDir $testProject.BaseName
  $testDllFile         = Join-Path $currentPublishDir $dllFileName;
  $buildCmd            = "dotnet build `"$testProject`" --configuration $configuration";
  Write-Output         "Running Build: $buildCmd"
  Invoke-Expression    $buildCmd;

  # Build published DLL file path
  $projectBaseDir      = Join-Path $testProject.Directory.FullName "\";
  $projectBinDir       = Join-Path $projectBaseDir "bin\";
  $projectBinCfgDir    = Join-Path $projectBinDir ($configuration + "\");
  $projectFrameworkDir = Join-Path $projectBinCfgDir ($frameworkVersion + "\");
  $testDllFile         = Join-Path $projectFrameworkDir $dllFileName;

  if(!(Test-Path $testDllFile)) {
    throw "Unable to find test DLL file: $testDllFile"
  }
  
  # Generate coverage report
  $testResultFileName   = Join-Path $testResultsDir "$( $testProject.BaseName )_results.trx";
  $coverageOutputDirTmp = Join-Path $testCoverageDir $testProject.BaseName;
  $coverageOutputDir    = Join-Path $coverageOutputDirTmp "\";

  # https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test
  $dotnetTestTargetArgs = @(
    "test",
    "$( $testProject.FullName )",
    "--logger:trx;LogFileName=$testResultFileName",
    "--configuration $configuration",
    "--no-build", 
    "--no-restore"
  );
  
  # https://www.jetbrains.com/help/dotcover/dotCover__Console_Runner_Commands.html
  $coverletDotnetArguments = @(
    "$testDllFile",
    "--target `"dotnet`"",
    "--targetargs `"$dotnetTestTargetArgs`"",
    "--output `"$coverageOutputDir`"",
    "--format `"cobertura`"",
    "--format `"opencover`""
  );

  $coverletCmd = "$coverletExe $coverletDotnetArguments";
  Write-Output "Executing Coverlet with following arguments: $coverletCmd";
  Invoke-Expression -Command $coverletCmd;
}
