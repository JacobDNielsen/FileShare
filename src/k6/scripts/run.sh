#!/usr/bin/env bash
set -euo pipefail

# -----------------------------------------------------------------------------
# Configuration block
# -----------------------------------------------------------------------------
# If false, values are read from the configuration block below and .env file. 
# If true, the user is prompted for values in the terminal, with defaults from the configuration block and .env.
INTERACTIVE_MODE=false 

TEST_NAME="auth_login"
PROTO="https"
SCENARIO="stress"
TARGET_URL=""               # optional. if set, it will override the .env URL resolution
ENV_FILE=".env"             
RESULTS_DIR="results"      
INSECURE_SKIP_TLS_VERIFY="" # optional. empty = use value from .env
EXTRA_K6_ARGS=(
  # Example:
  # --vus 5
  # --duration 10s
)
# -----------------------------------------------------------------------------

err() {
  printf 'ERROR: %s\n' "$*" >&2
}

fail() {
  err "$@"
  exit 1
}

info() {
  printf '%s\n' "$*"
}

upper() {
  printf '%s' "$1" | tr '[:lower:]' '[:upper:]'
}

require_command() {
  command -v "$1" >/dev/null 2>&1 || fail "Missing required command: $1"
}

resolve_project_root() {
  local script_dir="$1"
  local parent_dir=""
  local candidate=""

  parent_dir="$(cd "$script_dir/.." 2>/dev/null && pwd || true)"

  for candidate in "$script_dir" "$parent_dir" "$PWD"; do
    [[ -n "$candidate" ]] || continue

    if [[ -d "$candidate/tests" ]]; then
      printf '%s\n' "$(cd "$candidate" && pwd)"
      return 0
    fi
  done

  fail "Could not locate the k6 project root. Expected a tests/ directory next to this script, one level above it, or in the current working directory."
}

resolve_path_from_root() {
  local path_value="$1"

  if [[ -z "$path_value" ]]; then
    fail "Path value cannot be empty."
  fi

  if [[ "$path_value" == /* ]]; then
    printf '%s\n' "$path_value"
  else
    printf '%s\n' "$ROOT_DIR/$path_value"
  fi
}

load_env_file() {
  local env_file="$1"

  [[ -f "$env_file" ]] || fail "Missing env file: $env_file"

  while IFS= read -r line || [[ -n "$line" ]]; do
    line="${line%$'\r'}"

    [[ -z "$line" ]] && continue
    [[ "$line" =~ ^[[:space:]]*# ]] && continue

    if [[ "$line" =~ ^[[:space:]]*([A-Za-z_][A-Za-z0-9_]*)=(.*)$ ]]; then
      local key="${BASH_REMATCH[1]}"
      local value="${BASH_REMATCH[2]}"

      value="${value#"${value%%[![:space:]]*}"}"
      value="${value%"${value##*[![:space:]]}"}"

      if [[ "$value" =~ ^\"(.*)\"$ ]]; then
        value="${BASH_REMATCH[1]}"
      elif [[ "$value" =~ ^\'(.*)\'$ ]]; then
        value="${BASH_REMATCH[1]}"
      else
        value="${value%%[[:space:]]#*}"
        value="${value%"${value##*[![:space:]]}"}"
      fi

      export "$key=$value"
    else
      fail "Invalid line in env file: $line"
    fi
  done < "$env_file"
}

list_tests() {
  local tests_dir="$1"
  [[ -d "$tests_dir" ]] || fail "Tests directory not found: $tests_dir"

  local tests=()
  local file=""

  for file in "$tests_dir"/*.js; do
    [[ -e "$file" ]] || continue
    tests+=("$(basename "$file" .js)")
  done

  [[ ${#tests[@]} -gt 0 ]] || fail "No test files found in $tests_dir"

  printf '%s\n' "${tests[@]}" | sort
}

SCENARIOS=(stress spike breakpoint)

validate_proto() {
  case "$1" in
    http|https) ;;
    *) fail "Protocol must be http or https. Got: $1" ;;
  esac
}

validate_scenario() {
  local scenario="$1"
  local candidate=""

  for candidate in "${SCENARIOS[@]}"; do
    if [[ "$candidate" == "$scenario" ]]; then
      return 0
    fi
  done

  fail "Scenario must be one of: ${SCENARIOS[*]}. Got: $scenario"
}

normalize_bool() {
  local value="${1:-false}"

  case "${value,,}" in
    1|true|yes|y|on) printf 'true' ;;
    0|false|no|n|off|'') printf 'false' ;;
    *) fail "Boolean value must be one of: true/false/yes/no/1/0. Got: $value" ;;
  esac
}

is_interactive_terminal() {
  [[ -t 0 && -t 1 && -z "${CI:-}" ]]
}

prompt_input() {
  local var_name="$1"
  local label="$2"
  local default_value="${3:-}"
  local secret="${4:-false}"
  local allow_empty="${5:-false}"
  local value=""
  local prompt=""

  is_interactive_terminal || fail "Interactive mode requires a terminal."

  if [[ "$secret" == "true" ]]; then
    if [[ -n "$default_value" ]]; then
      prompt="$label [press Enter to keep current]: "
    elif [[ "$allow_empty" == "true" ]]; then
      prompt="$label [optional]: "
    else
      prompt="$label: "
    fi

    read -r -s -p "$prompt" value
    printf '\n'
  else
    if [[ -n "$default_value" ]]; then
      prompt="$label [$default_value]: "
    elif [[ "$allow_empty" == "true" ]]; then
      prompt="$label [optional]: "
    else
      prompt="$label: "
    fi

    read -r -p "$prompt" value
  fi

  if [[ -z "$value" ]]; then
    value="$default_value"
  fi

  if [[ -z "$value" && "$allow_empty" != "true" ]]; then
    fail "Value cannot be empty: $var_name"
  fi

  printf -v "$var_name" '%s' "$value"
}

prompt_select() {
  local var_name="$1"
  local label="$2"
  local current_value="$3"
  shift 3
  local options=("$@")
  local input=""
  local chosen=""
  local i=0
  local has_current=false

  is_interactive_terminal || fail "Interactive mode requires a terminal."

  for chosen in "${options[@]}"; do
    if [[ "$chosen" == "$current_value" ]]; then
      has_current=true
      break
    fi
  done

  printf '%s\n' "$label"
  for i in "${!options[@]}"; do
    if [[ "${options[$i]}" == "$current_value" && "$has_current" == "true" ]]; then
      printf '  %s) %s (default)\n' "$((i + 1))" "${options[$i]}"
    else
      printf '  %s) %s\n' "$((i + 1))" "${options[$i]}"
    fi
  done

  while true; do
    if [[ "$has_current" == "true" ]]; then
      read -r -p "Choose an option [1-${#options[@]}] or press Enter to keep '$current_value': " input
      if [[ -z "$input" ]]; then
        printf -v "$var_name" '%s' "$current_value"
        return 0
      fi
    else
      read -r -p "Choose an option [1-${#options[@]}]: " input
    fi

    if [[ "$input" =~ ^[0-9]+$ ]] && (( input >= 1 && input <= ${#options[@]} )); then
      chosen="${options[$((input - 1))]}"
      printf -v "$var_name" '%s' "$chosen"
      return 0
    fi

    for chosen in "${options[@]}"; do
      if [[ "$input" == "$chosen" ]]; then
        printf -v "$var_name" '%s' "$chosen"
        return 0
      fi
    done

    printf 'Invalid choice. Try again.\n' >&2
  done
}

resolve_service_name() {
  local test_name="$1"
  printf '%s\n' "${test_name%%_*}"
}

resolve_target_url() {
  local test_name="$1"
  local proto="$2"
  local target_url_override="${3:-}"

  if [[ -n "$target_url_override" ]]; then
    printf '%s\n' "$target_url_override"
    return 0
  fi

  local test_upper=""
  local proto_upper=""
  local service_name=""
  local service_upper=""
  local full_url_var=""
  local base_var=""
  local path_var=""
  local full_url=""
  local base_url=""
  local test_path=""

  test_upper="$(upper "$test_name")"
  proto_upper="$(upper "$proto")"
  service_name="$(resolve_service_name "$test_name")"
  service_upper="$(upper "$service_name")"

  full_url_var="${test_upper}_${proto_upper}"
  full_url="${!full_url_var:-}"
  if [[ -n "$full_url" ]]; then
    printf '%s\n' "$full_url"
    return 0
  fi

  base_var="${service_upper}_BASE_${proto_upper}"
  path_var="${test_upper}_PATH"
  base_url="${!base_var:-}"
  test_path="${!path_var:-}"

  [[ -n "$base_url" ]] || fail "Missing ${base_var}. Set it in .env or provide TARGET_URL directly."
  [[ -n "$test_path" ]] || fail "Missing ${path_var}. Set it in .env or provide TARGET_URL directly."

  base_url="${base_url%/}"
  [[ "$test_path" == /* ]] || test_path="/$test_path"

  printf '%s\n' "${base_url}${test_path}"
}

build_env_args() {
  ENV_ARGS=(
    -e "TARGET_URL=$TARGET_URL"
    -e "SCENARIO=$SCENARIO"
    -e "INSECURE_SKIP_TLS_VERIFY=$INSECURE_SKIP_TLS_VERIFY"
  )

  if [[ -n "${USERNAME:-}" ]]; then
    ENV_ARGS+=(-e "USERNAME=$USERNAME")
  fi

  if [[ -n "${PASSWORD:-}" ]]; then
    ENV_ARGS+=(-e "PASSWORD=$PASSWORD")
  fi
}

write_test_info() {
  local out_dir="$1"
  local test_file="$2"
  local env_file="$3"
  local git_commit="unknown"
  local k6_version="unknown"
  local service_name=""

  service_name="$(resolve_service_name "$TEST_NAME")"

  if command -v git >/dev/null 2>&1; then
    git_commit="$(git rev-parse --short HEAD 2>/dev/null || printf 'unknown')"
  fi

  k6_version="$(k6 version 2>/dev/null || printf 'unknown')"

  cat > "$out_dir/test_info.txt" <<EOF_INNER
timestamp=$TIMESTAMP
mode=$MODE_LABEL
test_name=$TEST_NAME
protocol=$PROTO
scenario=$SCENARIO
service=$service_name
target_url=$TARGET_URL
test_file=$test_file
env_file=$env_file
git_commit=$git_commit
k6_version=$k6_version
EOF_INNER
}

print_run_summary() {
  local service_name=""
  service_name="$(resolve_service_name "$TEST_NAME")"

  info "Running k6 test"
  info "  mode:      $MODE_LABEL"
  info "  test:      $TEST_NAME"
  info "  protocol:  $PROTO"
  info "  scenario:  $SCENARIO"
  info "  service:   $service_name (auto from test name)"
  info "  target:    $TARGET_URL"
  info "  env file:  $ENV_FILE"
  info "  results:   $OUT_DIR"
}

usage() {
  cat <<EOF_INNER
Usage:
  ./run.sh
  ./run.sh --interactive
  ./run.sh --manual
  ./run.sh --test auth_login --proto https --scenario stress
  ./run.sh --test auth_login --proto https --scenario stress -- --vus 5 --duration 10s

Modes:
  INTERACTIVE_MODE=false  Read values from the configuration block in this file.
  INTERACTIVE_MODE=true   Prompt for values in the terminal.

Options:
  --interactive           Force interactive mode for this run
  --manual                Force manual mode for this run
  --test, -t              Test file name without .js
  --proto, -p             Protocol: http or https
  --scenario, -s          Scenario: ${SCENARIOS[*]}
  --target-url            Full target URL, skips env-based URL resolution
  --env-file              Env file path. Relative paths are resolved from the k6 project root.
  --results-dir           Results directory path. Relative paths are resolved from the k6 project root.
  --list-tests            Print available tests and exit
  --help, -h              Show this help

Env lookup strategy:
  1) <TEST_NAME>_<PROTO>, e.g. AUTH_LOGIN_HTTPS
  2) Auto-detect service from test name prefix and use:
     <SERVICE>_BASE_<PROTO> + <TEST_NAME>_PATH
     e.g. auth_login -> AUTH_BASE_HTTPS + AUTH_LOGIN_PATH

Examples:
  ./run.sh
  ./run.sh --interactive
  ./run.sh --manual
  ./run.sh --test auth_login --proto https --scenario spike
  ./run.sh --interactive -- --vus 5 --duration 30s
EOF_INNER
}

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(resolve_project_root "$SCRIPT_DIR")"
TESTS_DIR="$ROOT_DIR/tests"
ENV_FILE="$(resolve_path_from_root "$ENV_FILE")"
RESULTS_DIR="$(resolve_path_from_root "$RESULTS_DIR")"

MODE_OVERRIDE=""
PASS_THROUGH_ARGS=()
POSITIONAL=()

while [[ $# -gt 0 ]]; do
  case "$1" in
    --interactive)
      MODE_OVERRIDE="true"
      shift
      ;;
    --manual)
      MODE_OVERRIDE="false"
      shift
      ;;
    -t|--test)
      [[ $# -ge 2 ]] || fail "Missing value after $1"
      TEST_NAME="$2"
      shift 2
      ;;
    -p|--proto)
      [[ $# -ge 2 ]] || fail "Missing value after $1"
      PROTO="$2"
      shift 2
      ;;
    -s|--scenario)
      [[ $# -ge 2 ]] || fail "Missing value after $1"
      SCENARIO="$2"
      shift 2
      ;;
    --target-url)
      [[ $# -ge 2 ]] || fail "Missing value after $1"
      TARGET_URL="$2"
      shift 2
      ;;
    --env-file)
      [[ $# -ge 2 ]] || fail "Missing value after $1"
      ENV_FILE="$2"
      shift 2
      ;;
    --results-dir)
      [[ $# -ge 2 ]] || fail "Missing value after $1"
      RESULTS_DIR="$2"
      shift 2
      ;;
    --list-tests)
      list_tests "$TESTS_DIR"
      exit 0
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    --)
      shift
      while [[ $# -gt 0 ]]; do
        PASS_THROUGH_ARGS+=("$1")
        shift
      done
      ;;
    -* )
      fail "Unknown option: $1"
      ;;
    *)
      POSITIONAL+=("$1")
      shift
      ;;
  esac
done

if [[ ${#POSITIONAL[@]} -ge 1 ]]; then
  TEST_NAME="${POSITIONAL[0]}"
fi
if [[ ${#POSITIONAL[@]} -ge 2 ]]; then
  PROTO="${POSITIONAL[1]}"
fi
if [[ ${#POSITIONAL[@]} -ge 3 ]]; then
  SCENARIO="${POSITIONAL[2]}"
fi

ENV_FILE="$(resolve_path_from_root "$ENV_FILE")"
RESULTS_DIR="$(resolve_path_from_root "$RESULTS_DIR")"

require_command k6
load_env_file "$ENV_FILE"

if [[ -n "$MODE_OVERRIDE" ]]; then
  INTERACTIVE_MODE="$MODE_OVERRIDE"
fi
INTERACTIVE_MODE="$(normalize_bool "$INTERACTIVE_MODE")"

if [[ "$INTERACTIVE_MODE" == "true" ]]; then
  MODE_LABEL="interactive"
else
  MODE_LABEL="manual"
fi

if [[ "$INTERACTIVE_MODE" == "true" ]]; then
  mapfile -t available_tests < <(list_tests "$TESTS_DIR")

  if [[ ${#available_tests[@]} -eq 1 ]]; then
    TEST_NAME="${available_tests[0]}"
  else
    prompt_select TEST_NAME "Available tests:" "$TEST_NAME" "${available_tests[@]}"
  fi

  prompt_select PROTO "Choose protocol:" "$PROTO" http https
  prompt_select SCENARIO "Choose scenario:" "$SCENARIO" "${SCENARIOS[@]}"
  prompt_input TARGET_URL "Target URL override for this run only (press Enter to use .env URL resolution)" "$TARGET_URL" false true

  if [[ -z "$TARGET_URL" ]]; then
    test_upper="$(upper "$TEST_NAME")"
    proto_upper="$(upper "$PROTO")"
    service_upper="$(upper "$(resolve_service_name "$TEST_NAME")")"
    full_url_var="${test_upper}_${proto_upper}"
    base_var="${service_upper}_BASE_${proto_upper}"
    path_var="${test_upper}_PATH"

    if [[ -z "${!full_url_var:-}" && -z "${!base_var:-}" ]]; then
      prompt_input base_input "Enter ${base_var}" "${!base_var:-}"
      export "$base_var=$base_input"
    fi

    if [[ -z "${!full_url_var:-}" && -z "${!path_var:-}" ]]; then
      prompt_input path_input "Enter ${path_var}" "${!path_var:-}"
      export "$path_var=$path_input"
    fi
  fi

  if [[ "$TEST_NAME" == auth_* ]]; then
    prompt_input USERNAME "Enter USERNAME" "${USERNAME:-}"
    prompt_input PASSWORD "Enter PASSWORD" "${PASSWORD:-}" true
  fi

  prompt_input INSECURE_SKIP_TLS_VERIFY "Skip TLS verification? (true/false)" "${INSECURE_SKIP_TLS_VERIFY:-false}"
else
  [[ -n "$TEST_NAME" ]] || fail "TEST_NAME cannot be empty in manual mode"
  [[ -n "$PROTO" ]] || fail "PROTO cannot be empty in manual mode"
  [[ -n "$SCENARIO" ]] || fail "SCENARIO cannot be empty in manual mode"

  if [[ "$TEST_NAME" == auth_* ]]; then
    [[ -n "${USERNAME:-}" ]] || fail "USERNAME is required for $TEST_NAME"
    [[ -n "${PASSWORD:-}" ]] || fail "PASSWORD is required for $TEST_NAME"
  fi
fi

validate_proto "$PROTO"
validate_scenario "$SCENARIO"
TEST_FILE="$TESTS_DIR/${TEST_NAME}.js"
[[ -f "$TEST_FILE" ]] || fail "Test file not found: $TEST_FILE"

TARGET_URL="$(resolve_target_url "$TEST_NAME" "$PROTO" "$TARGET_URL")"
INSECURE_SKIP_TLS_VERIFY="$(normalize_bool "${INSECURE_SKIP_TLS_VERIFY:-false}")"
export INSECURE_SKIP_TLS_VERIFY

TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
OUT_DIR="$RESULTS_DIR/$TEST_NAME/$PROTO/$SCENARIO/$TIMESTAMP"
mkdir -p "$OUT_DIR"

build_env_args
write_test_info "$OUT_DIR" "$TEST_FILE" "$ENV_FILE"
print_run_summary

k6 run \
  "${ENV_ARGS[@]}" \
  --summary-export "$OUT_DIR/summary.json" \
  --out "json=$OUT_DIR/metrics.json" \
  "${EXTRA_K6_ARGS[@]}" \
  "${PASS_THROUGH_ARGS[@]}" \
  "$TEST_FILE" | tee "$OUT_DIR/console.log"
