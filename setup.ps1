Param(
    [string]$ProjectName = "BrownianSim"
)

$ErrorActionPreference = "Stop"

Write-Host "=== BrownianSim Starter v5 ===" -ForegroundColor Cyan
Write-Host "ProjectName: $ProjectName"

try { dotnet --info | Out-Null } catch { Write-Host "dotnet SDK not found. Install .NET 9 SDK." -ForegroundColor Red; throw }

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

if (Test-Path $ProjectName) { Write-Host "Folder '$ProjectName' already exists. Remove it or pass -ProjectName NewName." -ForegroundColor Yellow; exit 1 }

Write-Host "Creating MAUI app..." -ForegroundColor Green
dotnet new maui -n $ProjectName

$dstRoot = Join-Path $root $ProjectName
$csproj  = Join-Path $dstRoot "$ProjectName.csproj"
$xml = Get-Content $csproj -Raw

if ($xml -match '<TargetFrameworks>.*?</TargetFrameworks>') { $xml = [regex]::Replace($xml, '<TargetFrameworks>.*?</TargetFrameworks>', '<TargetFrameworks>net9.0-windows10.0.19041.0</TargetFrameworks>') }
elseif ($xml -match '<TargetFramework>.*?</TargetFramework>') { $xml = [regex]::Replace($xml, '<TargetFramework>.*?</TargetFramework>', '<TargetFramework>net9.0-windows10.0.19041.0</TargetFramework>') }
else { $xml = $xml -replace '</PropertyGroup>', "<TargetFrameworks>net9.0-windows10.0.19041.0</TargetFrameworks>`n</PropertyGroup>" }
if ($xml -notmatch '<SupportedOSPlatformVersion>') { $xml = $xml -replace '</PropertyGroup>', '<SupportedOSPlatformVersion>10.0.19041.0</SupportedOSPlatformVersion></PropertyGroup>' }
Set-Content $csproj $xml -Encoding UTF8

Write-Host "Adding CommunityToolkit.Mvvm..." -ForegroundColor Green
dotnet add $ProjectName package CommunityToolkit.Mvvm

$solution = Join-Path $root "$ProjectName.sln"
dotnet new sln -n $ProjectName
dotnet sln $solution add (Join-Path $dstRoot "$ProjectName.csproj")

Write-Host "Creating Core library..." -ForegroundColor Green
$coreRoot = Join-Path $root "$ProjectName.Core"
dotnet new classlib -n "$ProjectName.Core" -f net9.0
dotnet sln $solution add (Join-Path $coreRoot "$ProjectName.Core.csproj")
dotnet add (Join-Path $dstRoot "$ProjectName.csproj") reference (Join-Path $coreRoot "$ProjectName.Core.csproj")

$srcRoot       = Join-Path $root "src_files"
$srcModels     = Join-Path $srcRoot "Models"
$srcViewModels = Join-Path $srcRoot "ViewModels"
$srcDrawables  = Join-Path $srcRoot "Drawables"
$srcViews      = Join-Path $srcRoot "Views"
$srcDocs       = Join-Path $srcRoot "docs"
$srcTests      = Join-Path $srcRoot "Tests"

$dstModels     = Join-Path $dstRoot "Models"
$dstViewModels = Join-Path $dstRoot "ViewModels"
$dstDrawables  = Join-Path $dstRoot "Drawables"
$dstViews      = Join-Path $dstRoot "Views"
$dstDocs       = Join-Path $dstRoot "docs"

New-Item -ItemType Directory -Force -Path $dstModels,$dstViewModels,$dstDrawables,$dstViews,$dstDocs | Out-Null

Copy-Item $srcViewModels -Destination $dstViewModels -Recurse -Force
Copy-Item $srcDrawables  -Destination $dstDrawables  -Recurse -Force
Copy-Item (Join-Path $srcViews "MainPage.xaml")    -Destination (Join-Path $dstViews "MainPage.xaml")   -Force
Copy-Item (Join-Path $srcViews "MainPage.xaml.cs") -Destination (Join-Path $dstViews "MainPage.xaml.cs") -Force
Copy-Item (Join-Path $srcRoot "AppShell.xaml")     -Destination (Join-Path $dstRoot "AppShell.xaml")     -Force
Copy-Item (Join-Path $srcRoot "AppShell.xaml.cs")  -Destination (Join-Path $dstRoot "AppShell.xaml.cs")  -Force
Copy-Item (Join-Path $srcRoot "App.xaml")          -Destination (Join-Path $dstRoot "App.xaml")          -Force
Copy-Item (Join-Path $srcRoot "App.xaml.cs")       -Destination (Join-Path $dstRoot "App.xaml.cs")       -Force
Copy-Item (Join-Path $srcRoot "MauiProgram.cs")    -Destination (Join-Path $dstRoot "MauiProgram.cs")    -Force
Copy-Item (Join-Path $srcRoot "README.md")         -Destination (Join-Path $dstRoot "README.md")         -Force
Copy-Item $srcDocs -Destination $dstDocs -Recurse -Force

$coreModels  = Join-Path $coreRoot "Models"
New-Item -ItemType Directory -Force -Path $coreModels | Out-Null
Copy-Item (Join-Path $srcModels "Brownian.cs") -Destination (Join-Path $coreModels "Brownian.cs") -Force

$filesApp = Get-ChildItem $dstRoot -Recurse -Include *.cs,*.xaml
foreach ($f in $filesApp) {
    $content = Get-Content $f.FullName -Raw
    $content = $content -replace 'BrownianSim', $ProjectName
    $content = $content -replace "using $ProjectName\.Models", "using $ProjectName.Core.Models"
    $content = $content -replace "clr-namespace:$ProjectName\.Views", "clr-namespace:$ProjectName.Views"
    Set-Content $f.FullName $content -Encoding UTF8
}

$coreBrownianPath = Join-Path $coreModels "Brownian.cs"
$coreContent = Get-Content $coreBrownianPath -Raw
$coreContent = $coreContent -replace 'namespace .*?\.Models;', "namespace $ProjectName.Core.Models;"
Set-Content $coreBrownianPath $coreContent -Encoding UTF8

Write-Host "Creating test project..." -ForegroundColor Green
$testsRoot = Join-Path $root "$ProjectName.Tests"
dotnet new xunit -n "$ProjectName.Tests" -f net9.0
dotnet sln $solution add (Join-Path $testsRoot "$ProjectName.Tests.csproj")
Copy-Item (Join-Path $srcTests "BrownianTests.cs") -Destination (Join-Path $testsRoot "BrownianTests.cs") -Force

$testFiles = Get-ChildItem $testsRoot -Recurse -Include *.cs
foreach ($f in $testFiles) {
    $t = Get-Content $f.FullName -Raw
    $t = $t -replace 'BrownianSim', $ProjectName
    $t = $t -replace "using $ProjectName\.Models", "using $ProjectName.Core.Models"
    Set-Content $f.FullName $t -Encoding UTF8
}

dotnet add (Join-Path $testsRoot "$ProjectName.Tests.csproj") reference (Join-Path $coreRoot "$ProjectName.Core.csproj")

Write-Host "Restoring workloads..." -ForegroundColor Green
dotnet workload restore

Write-Host "Building solution..." -ForegroundColor Green
dotnet build $solution -c Debug

Write-Host "Running tests..." -ForegroundColor Green
dotnet test $solution -c Debug

Write-Host "Running app..." -ForegroundColor Green
dotnet run --project (Join-Path $dstRoot "$ProjectName.csproj") -f net9.0-windows10.0.19041.0
