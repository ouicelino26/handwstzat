<#
.SYNOPSIS
  Verifies a release artifact by SHA-256 checksum.

.DESCRIPTION
  Computes the SHA-256 of a file and compares it against an expected value.
  Exits 0 if matching, 1 otherwise.

.PARAMETER Path
  Path to the artifact to verify.

.PARAMETER ExpectedSha256
  Expected SHA-256 hex string (64 chars, case-insensitive).

.EXAMPLE
  .\Verify-Artifact.ps1 -Path handwstat-1.2.0.msix -ExpectedSha256 abcd1234...
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Path,
    [Parameter(Mandatory)]
    [string]$ExpectedSha256
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Path)) {
    Write-Error "Artifact not found: $Path"
    exit 1
}

$hash = (Get-FileHash -Path $Path -Algorithm SHA256).Hash
Write-Host "File   : $Path"
Write-Host "SHA-256: $hash"

if ($hash.ToUpperInvariant() -eq $ExpectedSha256.ToUpperInvariant()) {
    Write-Host "VERIFY OK — checksum matches"
    exit 0
} else {
    Write-Error "VERIFY FAILED — expected: $($ExpectedSha256.ToUpperInvariant())"
    exit 1
}
