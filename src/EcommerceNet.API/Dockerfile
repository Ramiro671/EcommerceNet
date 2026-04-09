# ============================================================
# Dockerfile — EcommerceNet.API
# Multi-stage build: compilar con SDK, ejecutar con runtime ligero
# ============================================================

# Etapa 1: COMPILAR con el SDK completo (.NET 8)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiar archivos de proyecto primero (para cachear la capa de restore)
# Si no cambió ningún .csproj, Docker reutiliza la capa cacheada de restore
COPY EcommerceNet.slnx .
COPY src/EcommerceNet.Core/EcommerceNet.Core.csproj src/EcommerceNet.Core/
COPY src/EcommerceNet.Data/EcommerceNet.Data.csproj src/EcommerceNet.Data/
COPY src/EcommerceNet.API/EcommerceNet.API.csproj src/EcommerceNet.API/
COPY tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj tests/EcommerceNet.Tests/
RUN dotnet restore

# Copiar todo el código fuente y publicar en modo Release
COPY . .
RUN dotnet publish src/EcommerceNet.API -c Release -o /publish

# ============================================================
# Etapa 2: EJECUTAR con solo el ASP.NET runtime (imagen ligera)
# SDK pesa ~800MB, ASP.NET runtime pesa ~200MB
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copiar solo los binarios publicados de la etapa anterior
COPY --from=build /publish .

# Exponer el puerto HTTP (80 en contenedor)
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Variables de entorno por defecto (se sobreescriben en docker-compose)
ENV ASPNETCORE_ENVIRONMENT=Production

# Punto de entrada de la aplicación
ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
