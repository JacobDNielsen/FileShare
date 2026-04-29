# k6 Performance Test Runner

This folder contains a small k6-based test setup for comparing HTTP, HTTPS, and mTLS performance in the FileShare services.

## Purpose

- Compare HTTP vs. HTTPS vs. mTLS
- Compare default connection reuse vs. forced reconnects
- Run quick smoke tests and heavier stress-style tests
- Collect repeatable benchmark results

## Note about `run.sh`

The `run.sh` helper script was created with the use of Generative AI and then adapted for this project. Its only purpose is to automate k6 benchmark runs and save development time. In return, it may contain a slightly above-average amount of boilerplate code :-)

## Files

- `run.sh` — Interactive/manual runner for k6 tests
- `helpers/env.js` — Reads environment variables safely
- `helpers/scenarios.js` — Defines shared k6 scenarios and optional connection reuse overrides
- `tests/storage_direct_ping.js` — Baseline: k6 → storage ping directly
- `tests/gateway_storage_ping.js` — Baseline: k6 → gateway → storage ping
- `tests/storage_direct_upload_files.js` — Realistic: upload + list files directly on storage
- `tests/gateway_storage_upload_files.js` — Realistic: upload + list files via gateway
 - `tests/storage_direct_get_file.js` — Realistic: Get file contents directly from storage by FILE_ID
 - `tests/gateway_storage_get_file.js` — Realistic: Get file contents via gateway by FILE_ID
- `notebooks/metrics_visualization.ipynb` — Jupyter notebook for visualizing results

## Prerequisites

- [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/)
- A Bash-compatible shell (for example Git Bash on Windows)
- A `.env` file in this folder (copy from `.env.example`)

## Running tests

### With `run.sh`

```bash
./scripts/run.sh
```

Interactive mode prompts for test, protocol, and scenario. Or pass arguments directly:

```bash
./scripts/run.sh --test storage_direct_ping --proto https --scenario smoke
./scripts/run.sh --test gateway_storage_files --proto mtls --scenario stress
```

Results are saved to `results/{test}/{proto}/{scenario}/{timestamp}/`.

### Manually with k6

```bash
# Ping tests — only TARGET_URL needed
k6 run tests/storage_direct_ping.js \
  -e TARGET_URL=https://localhost:5135 \
  -e SCENARIO=smoke \
  -e INSECURE_SKIP_TLS_VERIFY=true

# File tests — also need credentials and path vars
k6 run tests/storage_direct_files.js \
  -e TARGET_URL=https://localhost:5135 \
  -e AUTH_URL=https://localhost:5040 \
  -e AUTH_LOGIN_PATH=/authentication/login \
  -e STORAGE_UPLOAD_PATH=/wopi/files/upload \
  -e STORAGE_LIST_PATH=/wopi/files \
  -e USERNAME=demo_user \
  -e PASSWORD=demo_user \
  -e INSECURE_SKIP_TLS_VERIFY=true
```

To run the new get-file tests (requires a pre-existing file id):

```bash
k6 run tests/storage_direct_get_file.js \
  -e TARGET_URL=https://localhost:5135 \
  -e AUTH_URL=https://localhost:5040 \
  -e AUTH_LOGIN_PATH=/authentication/login \
  -e STORAGE_DIRECT_GET_FILE_PATH=/wopi/files/<existing-file-id>/contents \
  -e FILE_ID=<existing-file-id> \
  -e USERNAME=demo_user \
  -e PASSWORD=demo_user \
  -e INSECURE_SKIP_TLS_VERIFY=true
```

```bash
k6 run tests/gateway_storage_get_file.js \
  -e TARGET_URL=https://localhost:8089 \
  -e GATEWAY_AUTH_URL=https://localhost:8089 \
  -e GATEWAY_AUTH_LOGIN_PATH=/api/auth/login \
  -e GATEWAY_STORAGE_GET_FILE_PATH=/api/storage/<existing-file-id>/contents \
  -e FILE_ID=<existing-file-id> \
  | `GATEWAY_AUTH_URL` | `gateway_storage_files` | Gateway auth base URL — must match the proto being tested |
  -e USERNAME=demo_user \
  -e PASSWORD=demo_user \
  -e INSECURE_SKIP_TLS_VERIFY=true
```

## Environment Variables

Copy `.env.example` to `.env` and fill in your values.

### URL resolution strategy (`run.sh`)

`run.sh` resolves `TARGET_URL` for each test using one of two strategies, in order of precedence:

1. **Full URL** — set `{TEST}_{PROTO}` directly, e.g. `STORAGE_DIRECT_FILES_HTTPS=https://localhost:5135`
2. **Base + path** — set `{SERVICE}_BASE_{PROTO}` and `{TEST}_PATH`, e.g. `STORAGE_BASE_HTTPS + STORAGE_DIRECT_PING_PATH`

Use strategy 1 to override a single test URL without touching the shared base. Use strategy 2 when multiple tests share the same service base URL.

### Variable reference

| Variable | Used by | Description |
|---|---|---|
| `USERNAME` | file tests | Login username |
| `PASSWORD` | file tests | Login password |
| `INSECURE_SKIP_TLS_VERIFY` | all | Skip TLS cert verification |
| `CLIENT_CERT_PATH` | mTLS | Path to client cert (PEM) |
| `CLIENT_KEY_PATH` | mTLS | Path to client key (PEM) |
| `AUTH_LOGIN_PATH` | `storage_direct_files` | Auth service login endpoint path |
| `AUTH_URL` | `storage_direct_files` | Auth service base URL — must match the proto being tested |
| `STORAGE_BASE_{PROTO}` | `storage_direct_ping` | Storage service base URL (base+path strategy) |
| `STORAGE_DIRECT_PING_PATH` | `storage_direct_ping` | Path to storage ping endpoint |
| `STORAGE_DIRECT_FILES_{PROTO}` | `storage_direct_files` | Storage service base URL (full URL strategy) |
| `STORAGE_DIRECT_GET_FILE_{PROTO}` | `storage_direct_get_file` | Storage service base URL for get-file (full URL strategy) |
| `STORAGE_DIRECT_GET_FILE_PATH` | `storage_direct_get_file` | Path to storage get-file endpoint (base+path strategy) |
| `STORAGE_UPLOAD_PATH` | `storage_direct_files` | Path to storage upload endpoint |
| `STORAGE_LIST_PATH` | `storage_direct_files` | Path to storage list endpoint |
| `GATEWAY_BASE_{PROTO}` | `gateway_storage_ping` | Gateway base URL (base+path strategy) |
| `GATEWAY_STORAGE_PING_PATH` | `gateway_storage_ping` | Path to storage ping via gateway |
| `GATEWAY_STORAGE_FILES_{PROTO}` | `gateway_storage_files` | Gateway base URL (full URL strategy) |
| `GATEWAY_STORAGE_GET_FILE_{PROTO}` | `gateway_storage_get_file` | Gateway base URL for get-file (full URL strategy) |
| `GATEWAY_STORAGE_GET_FILE_PATH` | `gateway_storage_get_file` | Path to gateway get-file endpoint (base+path strategy) |
| `FILE_ID` | `*_get_file` | Pre-existing file id used by get-file tests |
| `GATEWAY_AUTH_LOGIN_PATH` | `gateway_storage_files` | Path to auth login via gateway |
| `GATEWAY_STORAGE_UPLOAD_PATH` | `gateway_storage_files` | Path to storage upload via gateway |
| `GATEWAY_STORAGE_LIST_PATH` | `gateway_storage_files` | Path to storage list via gateway |
