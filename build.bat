@echo off
REM NoteD Build Script (Batch wrapper)
REM Usage: build.bat [Release|Debug] [clean]

setlocal

set CONFIG=Release
set CLEAN=

if /i "%1"=="Debug" set CONFIG=Debug
if /i "%1"=="Release" set CONFIG=Release
if /i "%2"=="clean" set CLEAN=-Clean
if /i "%1"=="clean" set CLEAN=-Clean

echo.
echo NoteD Build Script
echo ==================
echo Configuration: %CONFIG%
echo.

REM Check for PowerShell
where pwsh >nul 2>&1
if %ERRORLEVEL% equ 0 (
    pwsh -ExecutionPolicy Bypass -File "%~dp0build.ps1" -Configuration %CONFIG% %CLEAN%
    goto :end
)

where powershell >nul 2>&1
if %ERRORLEVEL% equ 0 (
    powershell -ExecutionPolicy Bypass -File "%~dp0build.ps1" -Configuration %CONFIG% %CLEAN%
    goto :end
)

REM Fallback to dotnet directly
echo PowerShell not found, using dotnet directly...
dotnet publish src\NoteD.Cli\NoteD.Cli.csproj -c %CONFIG% -o publish

:end
echo.
if exist publish\NoteD.exe (
    echo Build complete: publish\NoteD.exe
) else (
    echo Build may have failed - check output above
)

