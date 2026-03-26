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

### 1. Configure `.env`

```env
# Password used when generating the .pfx certificate
HTTPS_CERT_PASSWORD=your-password-here

# Protocol and port for service-to-service traffic inside Docker
# HTTP (no cert required):
INTERNAL_SERVICE_SCHEME=http
INTERNAL_SERVICE_PORT=8080

# HTTPS (requires cert setup - see https-certificate.md):
# INTERNAL_SERVICE_SCHEME=https
# INTERNAL_SERVICE_PORT=8443
```

If using `https` internally, complete the certificate setup first: [https-certificate.md](https-certificate.md).

### 2. Run the stack

```bash
docker compose up --build
```

#### Stopping and removing resources created by `docker compose up`:

```bash
docker compose down
```

### Overview of ports

| Service     | HTTP (host) | HTTPS (host) |
| ----------- | ----------- | ------------ |
| API Gateway | 8088        | 8089         |
| Auth        | 5039        | 5040         |
| Storage     | 5134        | 5135         |
| Lock        | 5038        | 5037         |
| WOPI Host   | 5018        | 5019         |

Both HTTP and HTTPS ports are always exposed regardless of `INTERNAL_SERVICE_SCHEME` (the variable only controls traffic _between_ services inside Docker.)

---

## Option B - `dotnet run` (per service)

Run each service individually. All five services must be started for the full stack to function.

### Prerequisites

- .NET 9
- PostgreSQL running locally (or expose the Docker databases via their host ports)
- `dotnet user-secrets` configured per service:
  - DB connection strings - see [database.md](database.md)
  - JWT signing key (Auth service only) - see [authentication_secret.md](authentication_secret.md)
- For HTTPS: development certificates must exist - see [https-certificate.md](https-certificate.md)

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

### Overview of ports

| Service     | HTTP port | HTTPS port |
| ----------- | --------- | ---------- |
| API Gateway | 8088      | 8089       |
| Auth        | 5039      | 5040       |
| Storage     | 5134      | 5135       |
| Lock        | 5038      | 5037       |
| WOPI Host   | 5018      | 5019       |

Start the gateway last (or allow it to retry) since it depends on the other services being up.

---
