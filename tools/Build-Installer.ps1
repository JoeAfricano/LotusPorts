# Builds the Lotus Ports installer: self-contained Release publish + Inno Setup.
# Run ONLY when the player (José) asks for an installer. It never installs anything on this PC.
# Usage: powershell -ExecutionPolicy Bypass -File tools\Build-Installer.ps1
# Output: artifacts\dist\LotusPorts-Setup-<version>.exe (+ .sha256). Needs Inno Setup 6 (winget install JRSoftware.InnoSetup).
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $root 'artifacts\dist\LotusPorts'
if (Test-Path $out) { Remove-Item $out -Recurse -Force }
dotnet publish (Join-Path $root 'src\LotusPorts.App\LotusPorts.App.csproj') -c Release -r win-x64 --self-contained true -p:Platform=x64 -p:WindowsAppSDKSelfContained=true -o $out -nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }
$exe = Join-Path $out 'LotusPorts.exe'
$version = (Get-Item $exe).VersionInfo.ProductVersion -replace '\+.*$', ''
$numeric = (Get-Item $exe).VersionInfo.FileVersion
$iscc = @("$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe", "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe", "$env:ProgramFiles\Inno Setup 6\ISCC.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { throw "Inno Setup 6 not found. Install it with: winget install JRSoftware.InnoSetup" }
& $iscc "/DAppVersion=$version" "/DNumericVersion=$numeric" "/DSourceDir=$out" "/DOutputDir=$(Join-Path $root 'artifacts\dist')" (Join-Path $PSScriptRoot 'installer\LotusPorts.iss')
if ($LASTEXITCODE -ne 0) { throw "ISCC failed ($LASTEXITCODE)" }
$setup = Join-Path $root "artifacts\dist\LotusPorts-Setup-$version.exe"
$hash = (Get-FileHash $setup -Algorithm SHA256).Hash
Set-Content -Path "$setup.sha256" -Value "$hash  $(Split-Path $setup -Leaf)" -Encoding ascii
Write-Output "Installer: $setup ($([math]::Round((Get-Item $setup).Length / 1MB)) MB)"
Write-Output "SHA256:    $hash"
