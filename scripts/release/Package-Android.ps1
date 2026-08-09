<#
.SYNOPSIS
  Builds the Android APK/AAB in Release mode.

.DESCRIPTION
  Runs dotnet build for net10.0-android in Release.
  Signing is NOT configured — SIGNING_STATUS=BLOCKED_EXTERNAL_CREDENTIALS.
  Output APK is in bin/Release/net10.0-android/.

.PARAMETER Version
  Application version string (e.g. "1.2.0").

.PARAMETER OutputApk
  Optional path to copy the unsigned APK to.

.EXAMPLE
  .\Package-Android.ps1 -Version 1.2.0
  .\Package-Android.ps1 -Version 1.2.0 -OutputApk artifacts/android/handwstat-1.2.0.apk
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Version,
    [string]$OutputApk = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Get-Item $PSScriptRoot).Parent.Parent.FullName

Write-Host "Packaging Android $Version (unsigned)"
Write-Host "NOTE: SIGNING_STATUS=BLOCKED_EXTERNAL_CREDENTIALS — APK is unsigned"

dotnet build "$repoRoot\HandWStat.csproj" `
    -c Release `
    -f net10.0-android `
    -p:ApplicationVersion=$Version `
    --no-restore

if ($LASTEXITCODE -ne 0) { throw "Android build failed" }

if ($OutputApk) {
    $apkDir = Join-Path $repoRoot "bin\Release\net10.0-android"
    $src = Get-ChildItem $apkDir -Filter "*.apk" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($src) {
        Copy-Item $src.FullName $OutputApk -Force
        Write-Host "APK copied to: $OutputApk"
    } else {
        Write-Warning "No APK found in $apkDir"
    }
}

Write-Host "Package-Android OK"
