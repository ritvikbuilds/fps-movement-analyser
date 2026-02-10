<#
.SYNOPSIS
    Build script for NoteD - FPS Input Analyzer

.DESCRIPTION
    Builds NoteD as a single-file self-contained Windows executable.

.PARAMETER Configuration
    Build configuration: Debug or Release (default: Release)

.PARAMETER Project
    Which project to build: Overlay (default), Cli, or All

.PARAMETER Clean
    Clean build output before building

.EXAMPLE
    .\build.ps1                          # Build overlay (main app)
    .\build.ps1 -Project Cli             # Build CLI version
    .\build.ps1 -Project All             # Build both
    .\build.ps1 -Configuration Debug
    .\build.ps1 -Clean
#>

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [ValidateSet("Overlay", "Cli", "All")]
    [string]$Project = "Overlay",
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

$ProjectRoot = $PSScriptRoot
$OverlayProject = Join-Path $ProjectRoot "src\NoteD.Overlay\NoteD.Overlay.csproj"
$CliProject = Join-Path $ProjectRoot "src\NoteD.Cli\NoteD.Cli.csproj"
$OutputDir = Join-Path $ProjectRoot "publish"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  NoteD Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Project: $Project | Config: $Configuration"
Write-Host ""

# Clean if requested
if ($Clean) {
    Write-Host "[1/4] Cleaning..." -ForegroundColor Yellow
    if (Test-Path $OutputDir) {
        Remove-Item -Recurse -Force $OutputDir
    }
    dotnet clean (Join-Path $ProjectRoot "NoteD.sln") -c $Configuration --nologo -v q
} else {
    Write-Host "[1/4] Skipping clean (use -Clean to clean first)" -ForegroundColor Gray
}

# Restore packages
Write-Host "[2/4] Restoring packages..." -ForegroundColor Yellow
dotnet restore (Join-Path $ProjectRoot "NoteD.sln") --nologo -v q

# Build projects
$projects = @()
if ($Project -eq "Overlay" -or $Project -eq "All") { $projects += @{Name="Overlay"; Path=$OverlayProject; Exe="NoteD.Overlay.exe"} }
if ($Project -eq "Cli" -or $Project -eq "All") { $projects += @{Name="CLI"; Path=$CliProject; Exe="NoteD.exe"} }

$step = 3
foreach ($proj in $projects) {
    Write-Host "[$step/4] Publishing $($proj.Name)..." -ForegroundColor Yellow
    
    $outDir = if ($projects.Count -gt 1) { Join-Path $OutputDir $proj.Name } else { $OutputDir }
    
    dotnet publish $proj.Path `
        -c $Configuration `
        -o $outDir `
        --nologo `
        -v q
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed for $($proj.Name)!" -ForegroundColor Red
        exit 1
    }
    $step++
}

# Show output
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

foreach ($proj in $projects) {
    $outDir = if ($projects.Count -gt 1) { Join-Path $OutputDir $proj.Name } else { $OutputDir }
    $ExePath = Join-Path $outDir $proj.Exe
    
    if (Test-Path $ExePath) {
        $FileInfo = Get-Item $ExePath
        $SizeMB = [math]::Round($FileInfo.Length / 1MB, 2)
        Write-Host "$($proj.Name):" -ForegroundColor White
        Write-Host "  Path: $ExePath" -ForegroundColor Gray
        Write-Host "  Size: $SizeMB MB" -ForegroundColor Gray
        Write-Host ""
    }
}

Write-Host "Run with: .\publish\NoteD.exe" -ForegroundColor Cyan


