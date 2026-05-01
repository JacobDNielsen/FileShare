# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file first (for caching)
COPY *.csproj ./
COPY --from=shared FileShare.Infrastructure/FileShare.Infrastructure.csproj ../shared/FileShare.Infrastructure/
RUN dotnet restore

# Copy everything else
COPY . ./
COPY --from=shared FileShare.Infrastructure/ ../shared/FileShare.Infrastructure/

# Restore + publish
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

COPY --from=ca rootCA.pem /usr/local/share/ca-certificates/mkcert-ca.crt
RUN update-ca-certificates

EXPOSE 8080
EXPOSE 8443

# IMPORTANT: update this if your compiled DLL name differs
ENTRYPOINT ["dotnet", "ApiGatewayYarp.dll"]