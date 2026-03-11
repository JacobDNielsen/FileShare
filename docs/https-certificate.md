# THIS IS WORK IN PROGRESS! - **Certificates currently don't work in Firefox!**

Docker commands:
docker compose up --build
docker compose down

--

# Install and setup mkcert

Install: winget install FiloSottile.mkcert
Install local CA for your host: mkcert -install

# Create .env file (in root):

Should contain: HTTPS_CERT_PASSWORD=verysecurepassword

# Create folders (do it in root):

New-Item -ItemType Directory -Force .\.dev-certs\pem | Out-Null
New-Item -ItemType Directory -Force .\.dev-certs\pfx | Out-Null
New-Item -ItemType Directory -Force .\.dev-certs\ca | Out-Null

# Certificate and key generation (PEM), used for localhost + docker:

mkcert -cert-file .\.dev-certs\pem\fileshare-dev.crt -key-file .\.dev-certs\pem\fileshare-dev.key localhost 127.0.0.1 ::1 auth storage lock wopi-host

# Convert them to .pfx, for ASP.NET Core Kestrel support (This requires OpenSSL):

openssl pkcs12 -export -out .\.dev-certs\pfx\fileshare-dev.pfx -inkey .\.dev-certs\pem\fileshare-dev.key -in .\.dev-certs\pem\fileshare-dev.crt -password pass:verysecurepassword

# Copy mkcert root CA for container trust (not yet supported)

$caroot = mkcert -CAROOT
Copy-Item "$caroot\rootCA.pem" .\.dev-certs\ca\rootCA.pem -Force
