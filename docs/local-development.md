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
INTERNAL_SERVICE_SCHEME=http
INTERNAL_SERVICE_PORT=8080
INTERNAL_SERVICE_USE_MTLS=false
```

#### HTTPS mode

Requires cert setup - see [https-certificate.md](https-certificate.md):

```env
DEV_SERVER_CERT_PASSWORD=your-password-here
INTERNAL_SERVICE_SCHEME=https
INTERNAL_SERVICE_PORT=8443
INTERNAL_SERVICE_USE_MTLS=false
```

#### mTLS mode

Requires cert setup - see [https-certificate.md](https-certificate.md):

```env
DEV_SERVER_CERT_PASSWORD=your-password-here
DEV_CLIENT_CERT_PASSWORD=your-password-here
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

### Inject certificate passwords via user secrets

Cert paths are set in `launchSettings.json`, but passwords should not be committed. Instead set them via user secrets - the value is the same password used when generating the certificate (see [https-certificate.md](https-certificate.md)).

**HTTPS and mTLS profiles** — both use `fileshare-server.pfx` as the server cert. The cert path is set per-profile in `launchSettings.json` via `Kestrel__Certificates__Default__Path`. The password is stored under `Kestrel:Certificates:Default:Password` in user secrets — this is under `Kestrel:Certificates`, not `Kestrel:Endpoints`, so it doesn't create a partial endpoint section and won't cause "endpoint X is missing Url" errors across profiles.

`Kestrel:Certificates:Default:Password` = value of `DEV_SERVER_CERT_PASSWORD` (all five services):

```powershell
dotnet user-secrets set "Kestrel:Certificates:Default:Password" "<your-password-here>" --project src/svc/auth/Auth.csproj
dotnet user-secrets set "Kestrel:Certificates:Default:Password" "<your-password-here>" --project src/svc/api-gateway-yarp/ApiGatewayYarp.csproj
dotnet user-secrets set "Kestrel:Certificates:Default:Password" "<your-password-here>" --project src/svc/storage/Storage.csproj
dotnet user-secrets set "Kestrel:Certificates:Default:Password" "<your-password-here>" --project src/svc/lock/Lock.csproj
dotnet user-secrets set "Kestrel:Certificates:Default:Password" "<your-password-here>" --project src/svc/wopi-host/WopiHost.csproj
```

**mTLS profile** - additionally requires the outbound client cert password (`fileshare-client.pfx` is a separate cert):


`Mtls:ClientCertPassword` = value of `DEV_CLIENT_CERT_PASSWORD` (storage, lock, wopi-host, gateway):

```powershell
dotnet user-secrets set "Mtls:ClientCertPassword" "<your-password-here>" --project src/svc/api-gateway-yarp/ApiGatewayYarp.csproj
dotnet user-secrets set "Mtls:ClientCertPassword" "<your-password-here>" --project src/svc/storage/Storage.csproj
dotnet user-secrets set "Mtls:ClientCertPassword" "<your-password-here>" --project src/svc/lock/Lock.csproj
dotnet user-secrets set "Mtls:ClientCertPassword" "<your-password-here>" --project src/svc/wopi-host/WopiHost.csproj
```

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

## Windows-specific notes

**curl:** Add `--ssl-no-revoke` to avoid Schannel certificate revocation errors with mkcert certs:

```bash
curl --ssl-no-revoke https://localhost:5040/benchmark/ping
```

**k6 / non-browser clients:** On Windows, use the IPv6 loopback `[::1]` instead of `localhost` — a system service (CDPSvc) can occupy the IPv4 address on certain ports, causing connection failures regardless of whether the server is running in Docker or via `dotnet run`:

```env
AUTH_BASE_HTTPS=https://[::1]:5040
```

---
