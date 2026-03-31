# FileShare Frontend

React + TypeScript frontend for the FileShare platform. Communicates exclusively through the YARP API Gateway.

## Tech Stack

- React 19 + TypeScript
- Vite 7 (dev server + build)
- React Router v7 (client-side routing)
- React Bootstrap (UI components)
- Axios (HTTP client)
- js-cookie (JWT persistence)

## Getting Started

```bash
npm install
npm run dev
```

Dev server runs on `http://localhost:5173` by default.

If `.dev-certs/pem/fileshare-dev.crt` and `.dev-certs/pem/fileshare-dev.key` exist (see root `docs/https-certificate.md`), the dev server automatically serves over **HTTPS** on `https://localhost:5173`.

The backend must be running — start it from the repo root with `docker compose up --build`.

## How API Calls Work

All requests go through Vite's dev proxy to the YARP API Gateway:

| Protocol | Gateway target |
|----------|---------------|
| HTTP     | `http://localhost:8088` |
| HTTPS    | `https://localhost:8089` |

Frontend code uses relative paths like `/api/storage/...` and Vite forwards them to the correct gateway port. JWT is stored in a cookie and automatically attached to every request via an Axios interceptor.

## Pages

| Route | Description |
|-------|-------------|
| `/login` | Login with username + password |
| `/signup` | Create a new account |
| `/files` | Paginated file list — upload, download, delete |
| `/files/:fileId` | File detail — rename, overwrite, lock management, WOPI URL |

## Scripts

```bash
npm run dev      # Start dev server
npm run build    # Production build (tsc + vite build)
npm run lint     # ESLint
npm run preview  # Preview production build
```
