# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file first (for caching)
COPY *.csproj ./
RUN dotnet restore

# Copy everything else
COPY . ./

# Restore + publish
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# IMPORTANT: update this if your compiled DLL name differs
ENTRYPOINT ["dotnet", "ApiGatewayYarp.dll"]