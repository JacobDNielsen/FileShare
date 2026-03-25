#Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Build context, from docker-compose (.src/svc/lock)
COPY ["Lock.csproj", "."]
RUN dotnet restore

COPY . .
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

ENTRYPOINT ["dotnet", "Lock.dll"]