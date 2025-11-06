# Multi-stage build für Angular Landing Page mit Caddy

# Stage 1: Build der Angular Anwendung
FROM node:20-alpine AS builder

WORKDIR /app

# Package-Dateien kopieren und Dependencies installieren
COPY package*.json ./
RUN npm ci

# Source-Code kopieren und Build ausführen
COPY . .
RUN npm run build

# Stage 2: Caddy Server für Production
FROM caddy:2-alpine

# Angular Build-Output von Builder-Stage kopieren
COPY --from=builder /app/dist/landing-page/browser /usr/share/caddy

# Caddyfile für SPA-Routing erstellen
RUN echo $'{\n\
    auto_https off\n\
    }\n\
    \n\
    :80 {\n\
    root * /usr/share/caddy\n\
    encode gzip\n\
    file_server\n\
    \n\
    # SPA Fallback: Alle nicht gefundenen Routen zu index.html\n\
    try_files {path} /index.html\n\
    \n\
    # Security Headers\n\
    header {\n\
    X-Content-Type-Options "nosniff"\n\
    X-Frame-Options "DENY"\n\
    X-XSS-Protection "1; mode=block"\n\
    Referrer-Policy "strict-origin-when-cross-origin"\n\
    }\n\
    \n\
    # Cache-Control für statische Assets\n\
    @static {\n\
    path *.js *.css *.png *.jpg *.jpeg *.gif *.webp *.svg *.ico *.woff *.woff2\n\
    }\n\
    header @static Cache-Control "public, max-age=31536000, immutable"\n\
    }' > /etc/caddy/Caddyfile

EXPOSE 80

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/ || exit 1

# Caddy läuft als non-root user
USER caddy
