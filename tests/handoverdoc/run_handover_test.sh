#!/bin/bash

echo "Kompiliere und führe HandoverDocumentation Test aus..."
echo

# Wechsle ins Test-Verzeichnis
cd "$(dirname "$0")"

# Kompiliere das eigenständige Test-Projekt
echo "Kompiliere das eigenständige Test-Projekt..."
dotnet build HandoverTestConsole.csproj

if [ $? -eq 0 ]; then
    echo "✓ Kompilierung erfolgreich"
    echo
    
    # Führe den Test aus
    echo "Führe HandoverDocumentation Test aus..."
    echo "================================================"
    dotnet run --project HandoverTestConsole.csproj --no-build
else
    echo "❌ Kompilierung fehlgeschlagen"
    read -p "Drücken Sie Enter zum Beenden..."
fi
