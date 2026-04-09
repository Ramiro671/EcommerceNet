# ============================================================
# Dockerfile raíz — para Elastic Beanstalk
# Multi-stage build: compilar con SDK, ejecutar con runtime ligero
# Contexto de build: raíz del repositorio (donde está EcommerceNet.sln)
# ============================================================

# Etapa 1: COMPILAR con el SDK completo
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiar archivos de proyecto primero (para cachear la capa de restore)
COPY EcommerceNet.slnx .
COPY src/EcommerceNet.Core/EcommerceNet.Core.csproj src/EcommerceNet.Core/
COPY src/EcommerceNet.Data/EcommerceNet.Data.csproj src/EcommerceNet.Data/
COPY src/EcommerceNet.API/EcommerceNet.API.csproj src/EcommerceNet.API/
COPY tests/EcommerceNet.Tests/EcommerceNet.Tests.csproj tests/EcommerceNet.Tests/
RUN dotnet restore

# Copiar todo el código y publicar en Release
COPY . .
RUN dotnet publish src/EcommerceNet.API -c Release -o /publish

# ============================================================
# Etapa 2: EJECUTAR con solo el ASP.NET runtime (~200 MB)
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /publish .

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "EcommerceNet.API.dll"]
