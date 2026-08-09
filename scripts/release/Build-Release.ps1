<#
.SYNOPSIS
  Builds all configured HandWStat release targets.

.DESCRIPTION
  Orchestrates restore + build for Windows and Android.
  Does NOT sign or package.

.PARAMETER Configuration
  Build configuration. Default: Release.

.PARAMETER Frameworks
  Target frameworks to build. Default: net10.0-windows10.0.19041.0, net10.0-android.

.EXAMPLE
  .\Build-Release.ps1
  .\Build-Release.ps1 -Frameworks net10.0-windows10.0.19041.0
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string[]]$Frameworks = @('net10.0-windows10.0.19041.0', 'net10.0-android')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Get-Item $PSScriptRoot).Parent.Parent.FullName
Write-Host "Repo: $repoRoot | Config: $Configuration | Targets: $($Frameworks -join ', ')"

Write-Host "[1/3] Restoring workloads..."
dotnet workload restore "$repoRoot\HandWStat.csproj"
if ($LASTEXITCODE -ne 0) { throw "Workload restore failed" }

Write-Host "[2/3] Restoring packages..."
dotnet restore "$repoRoot\HandWStat.slnx"
if ($LASTEXITCODE -ne 0) { throw "Package restore failed" }

Write-Host "[3/3] Building..."
foreach ($tfm in $Frameworks) {
    Write-Host "  -> $tfm"
    dotnet build "$repoRoot\HandWStat.csproj" -c $Configuration -f $tfm --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed: $tfm" }
}

Write-Host "Build-Release OK"
