#Build stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443
EXPOSE 9443

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Build context, from docker-compose (./src/svc/wopi-host)
COPY ["WopiHost.csproj", "."]
COPY --from=shared FileShare.Infrastructure/FileShare.Infrastructure.csproj ../shared/FileShare.Infrastructure/
RUN dotnet restore

COPY . .
COPY --from=shared FileShare.Infrastructure/ ../shared/FileShare.Infrastructure/
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

# Publish Image
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish ./

COPY --from=ca rootCA.pem /usr/local/share/ca-certificates/mkcert-ca.crt
RUN update-ca-certificates

ENTRYPOINT ["dotnet", "WopiHost.dll"]