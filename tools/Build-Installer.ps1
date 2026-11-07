# Builds the Lotus Ports installer: self-contained Release publish + Inno Setup.
# Run ONLY when the player (José) asks for an installer. It never installs anything on this PC.
# Usage: powershell -ExecutionPolicy Bypass -File tools\Build-Installer.ps1
# Output: artifacts\dist\LotusPorts-Setup-<version>.exe (+ .sha256). Needs Inno Setup 6 (winget install JRSoftware.InnoSetup).
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $root 'artifacts\dist\LotusPorts'
if (Test-Path $out) { Remove-Item $out -Recurse -Force }
# The app project sets EnableMsixTooling (its own .pri) and Lotus.Design keeps its theme as loose .xbf files; passing
# EnableMsixTooling here would reach the library and move its theme to the wrong path. Only pt-BR and en-US framework
# texts, no debug symbols.
dotnet publish (Join-Path $root 'src\LotusPorts.App\LotusPorts.App.csproj') -c Release -r win-x64 --self-contained true -p:Platform=x64 `
    -p:DebugType=none -p:DebugSymbols=false "-p:SatelliteResourceLanguages=en-US%3Bpt-BR" -o $out -nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }
if (-not (Test-Path (Join-Path $out 'LotusPorts.pri'))) { throw "LotusPorts.pri missing from the publish: the app would not open." }
# dotnet publish leaves out the library's loose compiled theme (Lotus.Design\*.xbf), which the app loads at start.
$design = Join-Path $root 'src\Lotus.Design\bin\Release\net10.0-windows10.0.26100.0\win-x64\Lotus.Design'
Copy-Item $design (Join-Path $out 'Lotus.Design') -Recurse -Force
if (-not (Test-Path (Join-Path $out 'Lotus.Design\LotusTheme.xbf'))) { throw "Lotus.Design\LotusTheme.xbf missing: the app would not open." }
$keepLanguages = @('en-US', 'pt-BR')
Get-ChildItem $out -Directory | Where-Object { $_.Name -match '^[a-z]{2,3}(-[A-Za-z]{2,4})*(-[A-Za-z0-9]{2,8})?$' -and $keepLanguages -notcontains $_.Name -and
    (Get-ChildItem $_.FullName -Recurse -File | Where-Object { $_.Extension -notin '.mui', '.dll' }).Count -eq 0 } | Remove-Item -Recurse -Force
foreach ($dev in 'createdump.exe', 'Microsoft.DiaSymReader.Native.amd64.dll') { Remove-Item (Join-Path $out $dev) -Force -ErrorAction SilentlyContinue }
# Windows App SDK AI features (on-device models, DirectML, vision, semantic search): Lotus Ports never uses them, and
# they are only loaded when an AI API is called. About 49 MB and dozens of files fewer.
$unusedAi = @('onnxruntime.dll', 'Microsoft.ML.OnnxRuntime.dll', 'DirectML.dll', 'Microsoft.Windows.AI.*', 'Microsoft.Windows.Internal.AI.*',
    'Microsoft.Windows.*SemanticSearch*', 'Microsoft.Asg.SemanticIndex*', 'Microsoft.Windows.Vision*', 'Microsoft.Windows.Internal.Vision*',
    'Microsoft.Graphics.Imaging*', 'Microsoft.Graphics.ImagingInternal*', 'Microsoft.Graphics.Internal.Imaging*',
    'Microsoft.Windows.Workloads*', 'Microsoft.Windows.Private.Workloads*', 'workloads*.json')
foreach ($pattern in $unusedAi) { Get-ChildItem $out -Filter $pattern -File | Remove-Item -Force }
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
