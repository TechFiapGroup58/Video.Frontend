# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/Video.Frontend.csproj ./src/
RUN dotnet restore ./src/Video.Frontend.csproj

COPY src/ ./src/
RUN dotnet publish ./src/Video.Frontend.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ── Stage 2: Serve com nginx ───────────────────────────────────────────────────
FROM nginx:alpine AS final

# Remove config padrão do nginx
RUN rm /etc/nginx/conf.d/default.conf

# Config customizada para SPA (todas as rotas → index.html)
COPY nginx.conf /etc/nginx/conf.d/app.conf

# Arquivos estáticos da publicação Blazor WASM
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

EXPOSE 80
