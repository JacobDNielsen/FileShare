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

-   `run.sh` --- Interactive/manual runner for k6 tests\
-   `helpers/env.js` --- Reads environment variables safely\
-   `helpers/scenarios.js` --- Defines shared k6 scenarios and optional
    connection reuse overrides\
-   `tests/storage_direct_ping.js` --- Baseline: k6 → storage ping
    directly\
-   `tests/gateway_storage_ping.js` --- Baseline: k6 → gateway → storage
    ping\
-   `tests/storage_direct_get_file.js` --- Get file contents directly
    from storage by FILE_ID\
-   `tests/gateway_storage_get_file.js` --- Get file contents via
    gateway by FILE_ID
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
./scripts/run.sh --test gateway_storage_get_file --proto mtls --scenario stress
```

### Manually with k6

#### Ping tests

``` bash
k6 run tests/storage_direct_ping.js \
  -e TARGET_URL=https://localhost:5135 \
  -e SCENARIO=smoke \
  -e INSECURE_SKIP_TLS_VERIFY=false
```

#### Get-file tests

``` bash
k6 run tests/storage_direct_get_file.js \
  -e TARGET_URL=https://localhost:5135 \
  -e AUTH_URL=https://localhost:5040 \
  -e AUTH_LOGIN_PATH=/authentication/login \
  -e STORAGE_DIRECT_GET_FILE_PATH=/wopi/files/<file-id>/download \
  -e FILE_ID=<file-id> \
  -e USERNAME=demo_user \
  -e PASSWORD=demo_user
```

``` bash
k6 run tests/gateway_storage_get_file.js \
  -e TARGET_URL=https://localhost:8089 \
  -e GATEWAY_AUTH_URL=https://localhost:8089 \
  -e GATEWAY_AUTH_LOGIN_PATH=/api/auth/login \
  -e GATEWAY_STORAGE_GET_FILE_PATH=/api/storage/<file-id>/download \
  -e FILE_ID=<file-id> \
  -e USERNAME=demo_user \
  -e PASSWORD=demo_user
```

## Environment Variables

### Core

  Variable                     Description
  ---------------------------- ----------------------------
  `USERNAME`                   Login username
  `PASSWORD`                   Login password
  `INSECURE_SKIP_TLS_VERIFY`   Skip TLS cert verification
  `CLIENT_CERT_PATH`           Path to client cert (mTLS)
  `CLIENT_KEY_PATH`            Path to client key (mTLS)

### Storage (direct)

  Variable                         Description
  -------------------------------- -------------------
  `STORAGE_BASE_{PROTO}`           Storage base URL
  `STORAGE_DIRECT_PING_PATH`       Ping endpoint
  `STORAGE_DIRECT_GET_FILE_PATH`   Get-file endpoint
  `AUTH_URL`                       Auth base URL
  `AUTH_LOGIN_PATH`                Auth login path

### Gateway

  Variable                          Description
  --------------------------------- -------------------------
  `GATEWAY_BASE_{PROTO}`            Gateway base URL
  `GATEWAY_STORAGE_PING_PATH`       Ping via gateway
  `GATEWAY_STORAGE_GET_FILE_PATH`   Get-file via gateway
  `GATEWAY_AUTH_URL`                Gateway auth base URL
  `GATEWAY_AUTH_LOGIN_PATH`         Gateway auth login path

### Test data

  Variable    Description
  ----------- -----------------------------------------
  `FILE_ID`   Existing file ID used in get-file tests
