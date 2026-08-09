<#
.SYNOPSIS
  Verifies that all signing prerequisites and toolchain are present.

.DESCRIPTION
  Checks for required signing environment variables (without revealing their values),
  .NET SDK version, installed MAUI workloads, and available disk space.
  Does NOT connect to any external service.

.EXAMPLE
  .\Test-ReleasePrerequisites.ps1
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Continue'

$errors   = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()
$passed   = [System.Collections.Generic.List[string]]::new()

function Pass  ([string]$msg) { $passed.Add("  [PASS] $msg") }
function Warn  ([string]$msg) { $warnings.Add("  [WARN] $msg") }
function Fail  ([string]$msg) { $errors.Add("  [FAIL] $msg") }
function CheckEnvVar ([string]$name, [string]$purpose) {
    $val = [System.Environment]::GetEnvironmentVariable($name)
    if ($val) { Pass "$name is set ($purpose)" }
    else       { Warn "$name is NOT set — $purpose will be skipped" }
}

Write-Host ""
Write-Host "=== HandWStat Release Prerequisites Check ==="
Write-Host "Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host ""

# --- .NET SDK ---
Write-Host "--- .NET SDK ---"
try {
    $dotnetVersion = (dotnet --version 2>&1).Trim()
    if ($dotnetVersion -match '^10\.') { Pass ".NET SDK $dotnetVersion (net10 required)" }
    else { Warn ".NET SDK $dotnetVersion — net10.0 recommended (current may work)" }
} catch {
    Fail ".NET SDK not found in PATH"
}

# --- MAUI Workloads ---
Write-Host ""
Write-Host "--- MAUI Workloads ---"
try {
    $workloads = (dotnet workload list 2>&1) | Select-String -Pattern '\b(maui|android|ios|maccatalyst)\b' -AllMatches
    $installed = $workloads | ForEach-Object { $_.Matches } | ForEach-Object { $_.Value } | Select-Object -Unique
    foreach ($w in @('android', 'ios', 'maccatalyst', 'maui-windows')) {
        if ($installed -contains $w) { Pass "Workload '$w' installed" }
        else { Warn "Workload '$w' not found — build for that platform may fail" }
    }
} catch {
    Warn "Could not verify workloads (dotnet workload list failed)"
}

# --- Disk space ---
Write-Host ""
Write-Host "--- Disk Space ---"
try {
    $drive = (Get-Item $PSScriptRoot).PSDrive
    $free  = [math]::Round($drive.Free / 1GB, 1)
    if ($free -ge 5) { Pass "Free space: ${free} GB (≥5 GB required for Android AOT)" }
    elseif ($free -ge 2) { Warn "Free space: ${free} GB — Android build may fail (needs ≥5 GB)" }
    else { Fail "Free space: ${free} GB — insufficient even for Windows build" }
} catch {
    Warn "Could not determine free disk space"
}

# --- Windows signing ---
Write-Host ""
Write-Host "--- Windows Signing Credentials ---"
CheckEnvVar 'HANDWSTAT_WINDOWS_CERTIFICATE_PATH'     'Windows MSIX signing (.pfx path)'
CheckEnvVar 'HANDWSTAT_WINDOWS_CERTIFICATE_PASSWORD' 'Windows MSIX signing (password)'

# --- Android signing ---
Write-Host ""
Write-Host "--- Android Signing Credentials ---"
CheckEnvVar 'HANDWSTAT_ANDROID_KEYSTORE_PATH'     'Android signing (keystore file path)'
CheckEnvVar 'HANDWSTAT_ANDROID_KEYSTORE_PASSWORD' 'Android signing (keystore password)'
CheckEnvVar 'HANDWSTAT_ANDROID_KEY_ALIAS'         'Android signing (key alias)'
CheckEnvVar 'HANDWSTAT_ANDROID_KEY_PASSWORD'      'Android signing (key password)'

# --- Summary ---
Write-Host ""
Write-Host "=== Summary ==="
$passed  | ForEach-Object { Write-Host $_ -ForegroundColor Green }
$warnings| ForEach-Object { Write-Host $_ -ForegroundColor Yellow }
$errors  | ForEach-Object { Write-Host $_ -ForegroundColor Red }
Write-Host ""
Write-Host "PASSED  : $($passed.Count)"
Write-Host "WARNINGS: $($warnings.Count)"
Write-Host "ERRORS  : $($errors.Count)"

if ($errors.Count -gt 0) {
    Write-Host ""
    Write-Host "Result: PREREQUISITES_FAILED — fix errors before releasing." -ForegroundColor Red
    exit 1
}
elseif ($warnings.Count -gt 0) {
    Write-Host ""
    Write-Host "Result: PREREQUISITES_PARTIAL — build possible but signing/platform coverage incomplete." -ForegroundColor Yellow
    exit 0
}
else {
    Write-Host ""
    Write-Host "Result: PREREQUISITES_OK" -ForegroundColor Green
    exit 0
}
