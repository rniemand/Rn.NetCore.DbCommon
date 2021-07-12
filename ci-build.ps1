param (
  $outputRoot = $PSScriptRoot
)

$outputRoot         = Join-Path $outputRoot "\";
$workingDir         = Join-Path $PSScriptRoot "\";
$sourceDir          = Join-Path $workingDir "src";
$artifactDir        = Join-Path $outputRoot "artifacts";
$publishDir         = Join-Path $artifactDir "publish";
$slnCommon          = Join-Path $sourceDir "Rn.NetCore.Common";
$slnWebCommon       = Join-Path $sourceDir "Rn.NetCore.WebCommon";
$slnMetricsRabbit   = Join-Path $sourceDir "Rn.NetCore.Metrics.Rabbit";
$buildConfiguration = "Release";
$buildCmd           = "";
$curPublishDir      = "";


# =============================================================================
# Build projects
# =============================================================================
#
$curPublishDir    = Join-Path $publishDir "Rn.NetCore.Common\";
$buildCmd         = "dotnet build $slnCommon --configuration $buildConfiguration --output `"$curPublishDir`""
Write-Host        "Running Build: $buildCmd"
Invoke-Expression $buildCmd;

$curPublishDir    = Join-Path $publishDir "Rn.NetCore.WebCommon\";
$buildCmd         = "dotnet build $slnWebCommon --configuration $buildConfiguration --output `"$curPublishDir`""
Write-Host        "Running Build: $buildCmd"
Invoke-Expression $buildCmd;

$curPublishDir    = Join-Path $publishDir "Rn.NetCore.Metrics.Rabbit\";
$buildCmd         = "dotnet build $slnMetricsRabbit --configuration $buildConfiguration --output `"$curPublishDir`""
Write-Host        "Running Build: $buildCmd"
Invoke-Expression $buildCmd;
