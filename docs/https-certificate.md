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

Copy `.env.example` to `.env` in the project root and fill in passwords for the server and client certificates:

```env
DEV_SERVER_CERT_PASSWORD=your-password-here   # fileshare-server.pfx
DEV_CLIENT_CERT_PASSWORD=your-password-here   # fileshare-client.pfx
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
  -cert-file .\.dev-certs\pem\fileshare-server.crt `
  -key-file  .\.dev-certs\pem\fileshare-server.key `
  localhost 127.0.0.1 ::1 auth storage lock wopi-host
```

---

## 5. Convert to PKCS12 (`.pfx`) for ASP.NET Core / Kestrel (This requires OpenSSL)

Use the value from `DEV_SERVER_CERT_PASSWORD` in your `.env` file:

```powershell
openssl pkcs12 -export `
  -out    .\.dev-certs\pfx\fileshare-server.pfx `
  -inkey  .\.dev-certs\pem\fileshare-server.key `
  -in     .\.dev-certs\pem\fileshare-server.crt `
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

After completing steps 1–6 your `.dev-certs/` directory should look like this:

```
.dev-certs/
├── ca/
│   └── rootCA.pem          # mkcert root CA
├── pem/
│   ├── fileshare-server.crt   # Server certificate (PEM)
│   └── fileshare-server.key   # Server private key (PEM)
└── pfx/
    └── fileshare-server.pfx   # Server certificate + key bundle for Kestrel
```

After also completing section 7 (client certificate for mTLS), the directory will also contain:

```
├── pem/
│   ├── fileshare-client.crt   # Client certificate (PEM)
│   └── fileshare-client.key   # Client private key (PEM)
└── pfx/
    └── fileshare-client.pfx   # Client certificate bundle for mTLS
```

The `.dev-certs/pfx/` directory is mounted read-only into every container at `/https/`. Services load `fileshare-server.pfx` as the server certificate and, when mTLS is enabled, load `fileshare-client.pfx` as the outbound client certificate.

---

## 7. Generate the shared client certificate for mTLS

All internal services (gateway, wopi-host, storage, lock) present this certificate when
connecting to any mTLS listener.

Generate the client certificate with mkcert:

```powershell
mkcert -client `
  -cert-file .\.dev-certs\pem\fileshare-client.crt `
  -key-file  .\.dev-certs\pem\fileshare-client.key `
  localhost 127.0.0.1 ::1 auth storage lock wopi-host
```

Bundle to PFX using the value from `DEV_CLIENT_CERT_PASSWORD`:

```powershell
openssl pkcs12 -export `
  -out    .\.dev-certs\pfx\fileshare-client.pfx `
  -inkey  .\.dev-certs\pem\fileshare-client.key `
  -in     .\.dev-certs\pem\fileshare-client.crt `
  -password pass:your-password-here
```

The `/https` Docker volume mount (`.dev-certs/pfx → /https`) makes `fileshare-client.pfx`
available at `/https/fileshare-client.pfx` inside every container automatically.

> **Production note**: Replace the single shared cert with per-service client certs and
> validate the connecting cert's CN or thumbprint using
> `AddCertificate().Events.OnCertificateValidated`.
