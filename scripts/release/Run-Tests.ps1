<#
.SYNOPSIS
  Runs the HandWStat test suite and outputs a summary.

.DESCRIPTION
  Executes dotnet test and produces a TRX report in artifacts/test-results/.
  Used locally to mirror what CI does.

.PARAMETER Configuration
  Build configuration. Default: Release.

.EXAMPLE
  .\Run-Tests.ps1
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot   = (Get-Item $PSScriptRoot).Parent.Parent.FullName
$resultsDir = Join-Path $repoRoot "artifacts\test-results"

if (-not (Test-Path $resultsDir)) { New-Item -ItemType Directory -Path $resultsDir | Out-Null }

Write-Host "Running tests ($Configuration)..."
dotnet test "$repoRoot\HandWStat.Tests\HandWStat.Tests.csproj" `
    -c $Configuration `
    --logger "trx;LogFileName=$resultsDir\test-results.trx" `
    --no-restore

if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
Write-Host "Run-Tests OK. Results: $resultsDir\test-results.trx"
