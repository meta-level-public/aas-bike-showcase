#!/bin/sh
# Script zum Generieren der Impressum-Konfiguration aus Umgebungsvariablen

CONFIG_DIR="/usr/share/caddy/assets/config"
CONFIG_FILE="$CONFIG_DIR/impressum.json"

# Erstelle Verzeichnis falls nicht vorhanden
mkdir -p "$CONFIG_DIR"

# Generiere JSON mit Umgebungsvariablen oder leeren Defaults
cat > "$CONFIG_FILE" << EOF
{
  "name": "${IMPRESSUM_NAME:-}",
  "street": "${IMPRESSUM_STREET:-}",
  "postalCode": "${IMPRESSUM_POSTAL_CODE:-}",
  "city": "${IMPRESSUM_CITY:-}",
  "country": "${IMPRESSUM_COUNTRY:-Deutschland}",
  "email": "${IMPRESSUM_EMAIL:-}",
  "phone": "${IMPRESSUM_PHONE:-}"
}
EOF

echo "Impressum-Konfiguration wurde erstellt: $CONFIG_FILE"
cat "$CONFIG_FILE"
