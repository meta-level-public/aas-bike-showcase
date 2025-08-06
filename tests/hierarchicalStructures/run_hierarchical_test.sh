#!/bin/bash

echo "Kompiliere und führe HierarchicalStructures Test aus..."
echo

# Wechsle ins Test-Verzeichnis
cd "$(dirname "$0")"

# Kompiliere das Test-Projekt
echo "Kompiliere das Test-Projekt..."
dotnet build HierarchicalTestConsole.csproj

if [ $? -eq 0 ]; then
    echo "✓ Kompilierung erfolgreich"
    echo
    
    # Führe den Test aus
    echo "Führe HierarchicalStructures Test aus..."
    echo "================================================"
    dotnet run --project HierarchicalTestConsole.csproj --no-build
else
    echo "❌ Kompilierung fehlgeschlagen"
    read -p "Drücken Sie Enter zum Beenden..."
fi
