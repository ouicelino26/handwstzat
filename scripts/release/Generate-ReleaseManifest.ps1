<#
.SYNOPSIS
  Generates a JSON release manifest for all artifacts in .artifacts/release/<version>/.

.DESCRIPTION
  Collects SHA-256 and file size for each artifact, captures build metadata,
  and writes a manifest JSON to .artifacts/release/manifest-<version>.json.

.PARAMETER Version
  Application version string (e.g. "1.2.0").

.PARAMETER Branch
  Git branch name. Defaults to current branch.

.PARAMETER Commit
  Git commit SHA. Defaults to HEAD.

.EXAMPLE
  .\Generate-ReleaseManifest.ps1 -Version 1.2.0
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Version,
    [string]$Branch = '',
    [string]$Commit = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot    = (Get-Item $PSScriptRoot).Parent.Parent.FullName
$artifactDir = Join-Path $repoRoot ".artifacts\release"

if (-not $Branch) {
    $Branch = (git -C $repoRoot branch --show-current 2>$null) ?? 'unknown'
}
if (-not $Commit) {
    $Commit = (git -C $repoRoot rev-parse HEAD 2>$null) ?? 'unknown'
}

$artifacts = @()

foreach ($platform in @('windows', 'android', 'ios')) {
    $dir = Join-Path $artifactDir $platform $Version
    if (-not (Test-Path $dir)) { continue }

    foreach ($file in Get-ChildItem $dir -Recurse -File) {
        $hash = (Get-FileHash $file.FullName -Algorithm SHA256).Hash
        $artifacts += @{
            platform    = $platform
            fileName    = $file.Name
            filePath    = $file.FullName.Substring($repoRoot.Length + 1)
            sha256      = $hash
            sizeBytes   = $file.Length
            signed      = $false  # update manually after signing
        }
    }
}

$manifest = @{
    version       = $Version
    buildDate     = (Get-Date -Format 'yyyy-MM-ddTHH:mm:ssZ')
    branch        = $Branch
    commit        = $Commit
    readyForRelease = $false
    signingStatus = 'BLOCKED_EXTERNAL_CREDENTIALS'
    artifacts     = $artifacts
}

$outPath = Join-Path $artifactDir "manifest-$Version.json"
$manifest | ConvertTo-Json -Depth 5 | Set-Content -Path $outPath -Encoding UTF8
Write-Host "Manifest written: $outPath"
Write-Host "Artifacts: $($artifacts.Count)"
