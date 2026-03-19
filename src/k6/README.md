# k6 Performance Test Runner

This folder contains a small k6-based test setup for comparing HTTP and HTTPS performance in the FileShare services.

## Purpose

The main goal is to make it easy to:

- compare HTTP vs. HTTPS
- compare default connection reuse vs. forced reconnects
- run quick smoke tests and heavier stress-style tests
- collect repeatable benchmark results

## Note about `run.sh`

The `run.sh` helper script was created with the use of Generative AI and then adapted for this project. Its only purpose is to automate k6 benchmark runs and save development time. In return, it may contain a slightly above-average amount of boilerplate code :-)

## Files

- `run.sh`  
  Interactive/manual runner for k6 tests.

- `helpers/env.js`  
  Reads environment variables safely.

- `helpers/scenarios.js`  
  Defines shared k6 scenarios and optional connection reuse overrides.

- `tests/auth_login.js`  
  Application-level benchmark for the auth login endpoint.

- `tests/auth_transport_ping.js` or `tests/transport_ping.js`  
  Minimal transport-focused benchmark endpoint test.

## Prerequisites

- [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/)
- A Bash-compatible shell (for example Git Bash on Windows)
- A `.env` file in this folder

## Environment Variables

Example `.env`:

```env
USERNAME=your-test-user
PASSWORD=your-test-password

AUTH_BASE_HTTP=http://localhost:5039
AUTH_BASE_HTTPS=https://localhost:5040

AUTH_LOGIN_PATH=/authentication/login
AUTH_TRANSPORT_PING_PATH=/benchmark/ping

INSECURE_SKIP_TLS_VERIFY=true
```
