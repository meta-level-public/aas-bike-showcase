@echo off
echo Kompiliere und fuehre HandoverDocumentation Test aus...
echo.

REM Wechsle ins Test-Verzeichnis
cd /d "%~dp0"

REM Kompiliere das eigenständige Test-Projekt
echo Kompiliere das eigenständige Test-Projekt...
dotnet build HandoverTestConsole.csproj

if %errorlevel% equ 0 (
    echo ✓ Kompilierung erfolgreich
    echo.
    
    REM Fuehre den Test aus
    echo Fuehre HandoverDocumentation Test aus...
    echo ================================================
    dotnet run --project HandoverTestConsole.csproj --no-build
) else (
    echo ❌ Kompilierung fehlgeschlagen
)

pause
