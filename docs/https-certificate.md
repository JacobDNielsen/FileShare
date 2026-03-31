# HTTPS Certificate Setup

> **Note:** Certificates (as of 26/03/2026) do not work in Firefox.

This documentation explains how to generate a self-signed dev certificate. The certificate covers both `localhost` and the internal Docker service hostnames.

---

## 1. Install mkcert

```powershell
winget install FiloSottile.mkcert
```

Then install the local Certificate Authority, ensuring that your OS trusts certificates it generates:

```powershell
mkcert -install
```

---

## 2. Create the `.env` file

Copy `.env.example` to `.env` in the project root and fill in a password. The password is used for both generating and loading the certificate:

```env
HTTPS_CERT_PASSWORD=your-password-here
```

---

## 3. Create the certificate folders

Run the following from the project root:

```powershell
New-Item -ItemType Directory -Force .\.dev-certs\pem | Out-Null
New-Item -ItemType Directory -Force .\.dev-certs\pfx | Out-Null
New-Item -ItemType Directory -Force .\.dev-certs\ca  | Out-Null
```

---

## 4. Generate the PEM certificate

The generated certificate cover `localhost`, the IPV4 (`127.0.0.1`) + IPV6 (`::1`) loopback addresses, and the Docker service hostnames (`auth`, `storage`, `lock`, `wopi-host`):

```powershell
mkcert `
  -cert-file .\.dev-certs\pem\fileshare-dev.crt `
  -key-file  .\.dev-certs\pem\fileshare-dev.key `
  localhost 127.0.0.1 ::1 auth storage lock wopi-host
```

---

## 5. Convert to PKCS12 (`.pfx`) for ASP.NET Core / Kestrel (This requires OpenSSL)

Use the same password as in the aforementioned `.env` file:

```powershell
openssl pkcs12 -export `
  -out    .\.dev-certs\pfx\fileshare-dev.pfx `
  -inkey  .\.dev-certs\pem\fileshare-dev.key `
  -in     .\.dev-certs\pem\fileshare-dev.crt `
  -password pass:your-password-here
```

---

## 6. Copy the root CA for Docker container trust

Containers need to trust the mkcert CA. The Docker images install this CA so HTTPS between services is trusted.

```powershell
$caroot = mkcert -CAROOT
Copy-Item "$caroot\rootCA.pem" .\.dev-certs\ca\rootCA.pem -Force
```

---

## Result

Your `.dev-certs/` directory should now look like this:

```
.dev-certs/
├── ca/
│   └── rootCA.pem          # mkcert root CA
├── pem/
│   ├── fileshare-dev.crt   # Certificate (PEM)
│   └── fileshare-dev.key   # Private key (PEM)
└── pfx/
    └── fileshare-dev.pfx   # Certificate + key bundle for Kestrel
```

The `.pfx` file and password are mounted into containers via `docker-compose.yml` and loaded by each service's Kestrel configuration.
