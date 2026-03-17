#!/usr/bin/env bash
set -euo pipefail

upper() { echo "$1" | tr '[:lower:]' '[:upper:]'; }

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

ENV_FILE="$ROOT_DIR/.env"
TESTS_DIR="$ROOT_DIR/tests"
RESULTS_DIR="$ROOT_DIR/results"

TEST_NAME="${1:-auth_login}"   # tests/<TEST_NAME>.js
PROTO="${2:-https}"            # https|http
SCENARIO="${3:-stress}"        # stress|spike|breakpoint etc.
shift $(( $# >= 3 ? 3 : $# ))  # remaining args are passed to k6

[[ -f "$ENV_FILE" ]] || {
  echo "Missing $ENV_FILE (copy .env.example -> .env)"
  exit 1
}

TEST_SCRIPT="$TESTS_DIR/${TEST_NAME}.js"
[[ -f "$TEST_SCRIPT" ]] || {
  echo "Missing test script: $TEST_SCRIPT"
  exit 1
}

case "$PROTO" in
  http|https) ;;
  *)
    echo "Invalid proto: $PROTO (use http|https)"
    exit 1
    ;;
esac

# Load env vars from .env
set -a
source "$ENV_FILE"
set +a

TEST_U="$(upper "$TEST_NAME")"
PROTO_U="$(upper "$PROTO")"

# 1) Prefer explicit full URL:
#    AUTH_LOGIN_HTTPS / AUTH_LOGIN_HTTP
FULL_URL_VAR="${TEST_U}_${PROTO_U}"
TARGET_URL="${!FULL_URL_VAR:-}"

# 2) Else build from:
#    <SERVICE>_BASE_<PROTO> + <TEST>_PATH
#    auth_login -> AUTH_BASE_HTTPS + AUTH_LOGIN_PATH
if [[ -z "$TARGET_URL" ]]; then
  SERVICE_PREFIX="${TEST_NAME%%_*}"           # auth_login -> auth
  SERVICE_U="$(upper "$SERVICE_PREFIX")"
  BASE_VAR="${SERVICE_U}_BASE_${PROTO_U}"     # AUTH_BASE_HTTPS
  ENDPOINT_PATH_VAR="${TEST_U}_PATH"          # AUTH_LOGIN_PATH

  BASE="${!BASE_VAR:-}"
  ENDPOINT_PATH="${!ENDPOINT_PATH_VAR:-}"

  if [[ -n "$BASE" && -n "$ENDPOINT_PATH" ]]; then
    BASE="${BASE%/}"
    [[ "$ENDPOINT_PATH" == /* ]] || ENDPOINT_PATH="/$ENDPOINT_PATH"
    TARGET_URL="${BASE}${ENDPOINT_PATH}"
  fi
fi

[[ -n "$TARGET_URL" ]] || {
  echo "Could not find TARGET_URL for test='$TEST_NAME' proto='$PROTO'."
  echo "Provide either:"
  echo "  1) ${FULL_URL_VAR}=https://host:port/path"
  echo "or:"
  echo "  2) <SERVICE>_BASE_${PROTO_U}=https://host:port"
  echo "     and ${TEST_U}_PATH=/path"
  exit 1
}

timestamp="$(date +"%Y%m%d-%H%M%S")"
OUT_DIR="$RESULTS_DIR/$TEST_NAME/$PROTO/$SCENARIO/$timestamp"
mkdir -p "$OUT_DIR"

{
  echo "timestamp=$timestamp"
  echo "test_name=$TEST_NAME"
  echo "protocol=$PROTO"
  echo "scenario=$SCENARIO"
  echo "target_url=$TARGET_URL"
  echo "k6_version=$(k6 version 2>/dev/null || true)"
  echo "git_commit=$(git rev-parse --short HEAD 2>/dev/null || true)"
} > "$OUT_DIR/test_info.txt"

echo "Running: $TEST_NAME  proto=$PROTO  scenario=$SCENARIO"
echo "URL: $TARGET_URL"
echo "Output: $OUT_DIR"

TARGET_URL="$TARGET_URL" \
SCENARIO="$SCENARIO" \
k6 run \
  --summary-export "$OUT_DIR/summary.json" \
  --out "json=$OUT_DIR/metrics.json" \
  "$@" \
  "$TEST_SCRIPT" | tee "$OUT_DIR/console.log"