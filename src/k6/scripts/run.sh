#!/usr/bin/env bash
set -euo pipefail

upper() {echo "$1" | tr '[:lower:]' '[:upper:]'; }

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENV_FILE="$ROOT_DIR/.env"
TEST_DIR="$ROOT_DIR/tests"
RESULTS_DIR="$ROOT_DIR/results"

TEST_NAME="${1:-auth_login}"
PROTOCOL="${2:-https}"
SCENARIO="${3:-stress}"

#  --- Validate inputs --- 
[[ -f "$ENV_FILE" ]] || { echo "Missing $ENV_FILE (copy k6/.env.example -> k6/.env)"; exit 1; }

TEST_SCRIPT="$TEST_DIR/${TEST_NAME}.js"
[[ -f "$TEST_SCRIPT" ]] || { echo "Test script not found: $TEST_SCRIPT"; exit 1; }

case "$PROTOCOL" in
  http|https) ;;
  *) echo "Invalid protocol: $PROTOCOL (must be 'http' or 'https')"; exit 1 ;;
esac

#  --- Loading the env vars --- 
set -a 
source "$ENV_FILE"
set +a

# --- Resolve the target URL ---
TEST_UPPER=$(upper "$TEST_NAME")
PROTOCOL_UPPER=$(upper "$PROTOCOL")
FULL_URL_VARIABLE="${TEST_UPPER}_${PROTOCOL_UPPER}"
TARGET_URL="${!FULL_URL_VARIABLE:-}"

# If no target url is found, it is build from this convention instead: <SERVICE>_BASE_<PROTOCOL> + <TEST>_PATH
# EXAMPLE:
#   AUTH_BASE_HTTPS=https://localhost:5040
#   AUTH_LOGIN_PATH=/authentication/login

if [[ -z "$TARGET_URL" ]]; then
    SERVICE_PREFIX = "${TEST_NAME%%_*}" #auth_login -> auth
    SERVICE_UPPER=$(upper "$SERVICE_PREFIX") 
    BASE_VARIABLE="${SERVICE_UPPER}_BASE_${PROTOCOL_UPPER}"
    PATH_VARIABLE="${TEST_UPPER}_PATH"

    BASE_URL="${!BASE_VARIABLE:-}"
    PATH_URL="${!PATH_VARIABLE:-}"

    if [[ -n "$BASE_URL" || -n "$PATH_URL" ]]; then
        BASE_URL="${BASE_URL%/}" # Remove trailing slash if exists
        [[ "$PATH_URL" == /* ]] || PATH_URL="/$PATH_URL" # Ensure path starts with a slash
        TARGET_URL="${BASE_URL}${PATH_URL}"
    fi
fi

[[ -n "$TARGET_URL" ]] || {
    echo "Target URL not found for $TEST_NAME with protocol $PROTOCOL. Please check your .env configuration."; exit 1; }
    echo "Provide either:" >&2
    echo " 1) $FULL_URL_VARIABLE=<full-url>" >&2
    echo "  for example: $FULL_URL_VARIABLE=https://localhost:5040/authentication/login" >&2
    echo " or both:" >&2
    echo " 2) $(upper "${TEST_NAME%%_*}")_BASE_${PROTOCOL_UPPER}=<base-url> and ${TEST_UPPER}_PATH=<path>" >&2
    echo " for example: AUTH_BASE_${PROTOCOL_UPPER}=https://localhost:5040 and ${TEST_UPPER}_PATH=/authentication/login" >&2
    exit 1
}

# --- Ouput directory ---
timestamp =$(date +"%Y%m%d-%H%M%S")
OUTPUT_DIR="$RESULTS_DIR/${TEST_NAME}_${PROTOCOL}_${SCENARIO}_${timestamp}"
mkdir -p "$OUTPUT_DIR"

echo "Running test: $TEST_NAME ($PROTOCOL) with scenario: $SCENARIO"
echo "Target URL: $TARGET_URL"
echo "Output directory: $OUTPUT_DIR"

# --- Run the k6 test ---
TARGET_URL="$TARGET_URL" \
SCENARIO="$SCENARIO" \
k6 run \
--summary-export="$OUTPUT_DIR/summary.json" \
--out json="$OUTPUT_DIR/metrics.json" "$TEST_SCRIPT" \
"$TEST_SCRIPT" | tee "$OUTPUT_DIR/output.log"