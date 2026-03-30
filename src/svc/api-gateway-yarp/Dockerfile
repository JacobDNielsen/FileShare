# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file first (for caching)
COPY src/svc/api-gateway-yarp/*.csproj ./
RUN dotnet restore

# Copy everything else
COPY src/svc/api-gateway-yarp/ ./

# Publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

COPY .dev-certs/ca/rootCA.pem /usr/local/share/ca-certificates/mkcert-ca.crt
RUN update-ca-certificates

EXPOSE 8080
EXPOSE 8443

# IMPORTANT: update this if your compiled DLL name differs
ENTRYPOINT ["dotnet", "ApiGatewayYarp.dll"]