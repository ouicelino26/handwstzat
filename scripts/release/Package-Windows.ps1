<#
.SYNOPSIS
  Publishes the Windows self-contained package.

.DESCRIPTION
  Runs dotnet publish for net10.0-windows10.0.19041.0 in Release.
  WindowsPackageType=None (unsigned).
  Output goes to artifacts/windows/.

.PARAMETER Version
  Application version string (e.g. "1.2.0").

.EXAMPLE
  .\Package-Windows.ps1 -Version 1.2.0
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Get-Item $PSScriptRoot).Parent.Parent.FullName
$outDir   = Join-Path $repoRoot "artifacts\windows\$Version"

Write-Host "Packaging Windows $Version -> $outDir"

dotnet publish "$repoRoot\HandWStat.csproj" `
    -c Release `
    -f net10.0-windows10.0.19041.0 `
    -p:WindowsPackageType=None `
    -p:ApplicationVersion=$Version `
    -o "$outDir" `
    --no-restore

if ($LASTEXITCODE -ne 0) { throw "Windows publish failed" }

Write-Host "Package-Windows OK: $outDir"
