@echo off
echo Kompiliere und fuehre HierarchicalStructures Test aus...
echo.

REM Wechsle ins Projektverzeichnis
cd /d "%~dp0"

REM Kompiliere das Projekt
echo Kompiliere das Projekt...
dotnet build

if %errorlevel% equ 0 (
    echo ✓ Kompilierung erfolgreich
    echo.
    
    REM Fuehre den Test aus
    echo Fuehre HierarchicalStructures Test aus...
    echo ================================================
    dotnet run --project . --no-build Production/HierarchicalStructuresTestProgram.cs
) else (
    echo ❌ Kompilierung fehlgeschlagen
)

pause
