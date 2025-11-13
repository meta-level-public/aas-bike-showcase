#!/bin/sh
# Script zum Generieren der Impressum-Konfiguration aus Umgebungsvariablen

CONFIG_DIR="/usr/share/caddy/assets/config"
CONFIG_FILE="$CONFIG_DIR/impressum.json"

# Erstelle Verzeichnis falls nicht vorhanden
mkdir -p "$CONFIG_DIR"

# Generiere JSON mit Umgebungsvariablen oder Defaults
cat > "$CONFIG_FILE" << EOF
{
  "name": "${IMPRESSUM_NAME:-Christina Bornträger}",
  "street": "${IMPRESSUM_STREET:-Hintereckstr. 10}",
  "postalCode": "${IMPRESSUM_POSTAL_CODE:-66606}",
  "city": "${IMPRESSUM_CITY:-St. Wendel}",
  "country": "${IMPRESSUM_COUNTRY:-Deutschland}",
  "email": "${IMPRESSUM_EMAIL:-borntraegerchristina@gmail.com}",
  "phone": "${IMPRESSUM_PHONE:-}"
}
EOF

echo "Impressum-Konfiguration wurde erstellt: $CONFIG_FILE"
cat "$CONFIG_FILE"
