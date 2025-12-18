Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$frontendRoot = Join-Path $repoRoot "frontend"
$hostWww = Join-Path $repoRoot "src\\DesktopHostWpf\\www"

$candidateBuildDirs = @(
  (Join-Path $frontendRoot "build"), # CRA
  (Join-Path $frontendRoot "dist")   # Vite
)

$buildDir = $candidateBuildDirs | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $buildDir) {
  throw "No build output found. Expected '$($candidateBuildDirs -join "', '")'. Run 'npm run build' first."
}

Write-Host "Syncing frontend from: $buildDir"
Write-Host "Syncing to host www:     $hostWww"

if (-not (Test-Path $hostWww)) {
  New-Item -ItemType Directory -Path $hostWww | Out-Null
}

Get-ChildItem -Path $hostWww -Force | Remove-Item -Recurse -Force
Copy-Item -Path (Join-Path $buildDir "*") -Destination $hostWww -Recurse -Force

Write-Host "Done."

