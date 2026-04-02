# Running the project

There are two supported ways to run the project locally:

- **Option A:** run the full stack with Docker Compose
- **Option B:** run services individually with `dotnet run`

The repository also includes a minimal React frontend under `src/web/frontend`, which can be started separately.

#### Running the frontend

```bash
cd src/web/frontend
npm install
npm run dev
```

---

## Option A - Docker Compose (recommended)

Starts all services, databases, and the API Gateway in one command.

### Prerequisites

- Docker with Docker Compose
- `kid1.pem` in the project root (see [authentication_secret.md](authentication_secret.md))
- `.env` file in the project root (copy from `.env.example`)
- HTTPS certificates generated (see [https-certificate.md](https-certificate.md))

### 1. Configure `.env`

The project supports three internal transport modes. Set the `.env` file for the mode you want:

#### HTTP mode

```env
DEV_CERT_PASSWORD=your-password-here
INTERNAL_SERVICE_SCHEME=http
INTERNAL_SERVICE_PORT=8080
INTERNAL_SERVICE_USE_MTLS=false
```

#### HTTPS mode

Requires cert setup - see [https-certificate.md](https-certificate.md):

```env
DEV_CERT_PASSWORD=your-password-here
INTERNAL_SERVICE_SCHEME=https
INTERNAL_SERVICE_PORT=8443
INTERNAL_SERVICE_USE_MTLS=false
```

#### mTLS mode

Requires cert setup - see [https-certificate.md](https-certificate.md):

```env
DEV_CERT_PASSWORD=your-password-here
INTERNAL_SERVICE_SCHEME=https
INTERNAL_SERVICE_PORT=9443
INTERNAL_SERVICE_USE_MTLS=true
```

### 2. Run the stack

The same command is used for all three modes:

```bash
docker compose up --build
```

#### Stopping and removing resources created by `docker compose up`:

```bash
docker compose down
```

### Overview of ports

| Service     | HTTP (host) | HTTPS (host) | mTLS (host) |
| ----------- | ----------- | ------------ | ----------- |
| API Gateway | 8088        | 8089         | —           |
| Auth        | 5039        | 5040         | 5041        |
| Storage     | 5134        | 5135         | 5136        |
| Lock        | 5038        | 5037         | 5036        |
| WOPI Host   | 5018        | 5019         | 5020        |

All ports are always exposed to the host regardless of which mode is active. In Docker, the `.env` variables control which port services use when talking to each other. With `dotnet run`, the launch profile (`http`, `https`, or `mtls`) controls this instead. Both HTTPS and mTLS modes require certificates — see [https-certificate.md](https-certificate.md).

---

## Option B - `dotnet run` (per service)

Run each service individually. All five services must be started for the full stack to function.

### Prerequisites

- .NET 9
- PostgreSQL running locally (or expose the Docker databases via their host ports)
- `dotnet user-secrets` configured per service:
  - DB connection strings - see [database.md](database.md)
  - JWT signing key (Auth service only) - see [authentication_secret.md](authentication_secret.md)
- For HTTPS/mTLS: development certificates must exist - see [https-certificate.md](https-certificate.md)

### HTTP profile

```bash
cd src/svc/<service-name>
dotnet run --launch-profile http
```

The gateway's `http` profile points all downstream clusters to `http://localhost:{port}` (already the default in `appsettings.Development.json`).

### HTTPS profile

```bash
cd src/svc/<service-name>
dotnet run --launch-profile https
```

The gateway's `https` profile overrides each cluster address to the corresponding HTTPS port via environment variables in `launchSettings.json` - no manual configuration needed.

### mTLS profile

```bash
cd src/svc/<service-name>
dotnet run --launch-profile mtls
```

The `mtls` profile configures each downstream service with three Kestrel endpoints (HTTP, HTTPS, and mTLS with `RequireCertificate`). The gateway's `mtls` profile points cluster destinations to the mTLS ports and loads the client certificate from `.dev-certs/pfx/fileshare-client.pfx`.

All five services must use the `mtls` profile. The client certificate must exist before starting (see [https-certificate.md](https-certificate.md)).

#### Inject the shared client-cert password via user secrets

For local `dotnet run`, configure the shared `DEV_CERT_PASSWORD` secret in each mTLS-enabled service so the client certificate can be loaded at runtime:

```powershell
dotnet user-secrets set "DEV_CERT_PASSWORD" "<your-password>" --project src/svc/api-gateway-yarp/ApiGatewayYarp.csproj
dotnet user-secrets set "DEV_CERT_PASSWORD" "<your-password>" --project src/svc/storage/Storage.csproj
dotnet user-secrets set "DEV_CERT_PASSWORD" "<your-password>" --project src/svc/lock/Lock.csproj
dotnet user-secrets set "DEV_CERT_PASSWORD" "<your-password>" --project src/svc/wopi-host/WopiHost.csproj
```

### Overview of ports

| Service     | HTTP port | HTTPS port | mTLS port |
| ----------- | --------- | ---------- | --------- |
| API Gateway | 8088      | 8089       | —         |
| Auth        | 5039      | 5040       | 5041      |
| Storage     | 5134      | 5135       | 5136      |
| Lock        | 5038      | 5037       | 5036      |
| WOPI Host   | 5018      | 5019       | 5020      |

Start the gateway last (or allow it to retry) since it depends on the other services being up.

---
